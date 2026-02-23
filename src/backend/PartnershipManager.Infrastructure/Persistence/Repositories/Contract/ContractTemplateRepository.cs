// F3-REP-001: ContractTemplateRepository Implementation
// File: src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/Contract/ContractTemplateRepository.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for reusable contract templates using Dapper
/// </summary>
public class ContractTemplateRepository : IContractTemplateRepository
{
    private readonly DapperContext _context;
    
    static ContractTemplateRepository()
    {
        // Configure custom type mapping for ContractTemplate to handle enum conversion
        SqlMapper.SetTypeMap(
            typeof(ContractTemplate),
            new CustomPropertyTypeMap(
                typeof(ContractTemplate),
                (type, columnName) => type.GetProperty(columnName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase)!
            )
        );
    }

    public ContractTemplateRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;
    
    /// <summary>
    /// Helper method to parse TemplateType enum from database value
    /// </summary>
    private static ContractTemplateType ParseTemplateType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ContractTemplateType.Other;
            
        // Try to find enum value by EnumMember attribute
        foreach (var field in typeof(ContractTemplateType).GetFields())
        {
            var attribute = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            if (attribute != null && attribute.Value == value)
            {
                return (ContractTemplateType)field.GetValue(null)!;
            }
        }

        // Fallback: try parse by name (case-insensitive)
        if (Enum.TryParse<ContractTemplateType>(value, true, out var result))
        {
            return result;
        }

        return ContractTemplateType.Other;
    }

    private const string SelectColumns = @"
        ct.id AS Id,
        ct.client_id AS ClientId,
        ct.company_id AS CompanyId,
        ct.name AS Name,
        ct.description AS Description,
        ct.code AS Code,
        ct.template_type AS TemplateType,
        ct.content AS Content,
        ct.default_status AS DefaultStatus,
        ct.tags AS Tags,
        ct.version AS Version,
        ct.is_active AS IsActive,
        ct.created_at AS CreatedAt,
        ct.updated_at AS UpdatedAt,
        ct.created_by AS CreatedBy,
        ct.updated_by AS UpdatedBy,
        ct.is_deleted AS IsDeleted,
        ct.deleted_at AS DeletedAt";

    private const string TableName = "contract_templates";

    public async Task<(IEnumerable<ContractTemplate> Items, int Total)> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        string? templateType = null,
        bool? isActive = null)
    {
        var offset = (page - 1) * pageSize;
        
        var whereClause = new StringBuilder("WHERE ct.client_id = @ClientId AND ct.is_deleted = 0");
        var parameters = new DynamicParameters();
        parameters.Add("@ClientId", clientId.ToString());

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Append(" AND (ct.name LIKE @Search OR ct.description LIKE @Search OR ct.code LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (!string.IsNullOrWhiteSpace(templateType))
        {
            whereClause.Append(" AND ct.template_type = @TemplateType");
            parameters.Add("@TemplateType", templateType);
        }

        if (isActive.HasValue)
        {
            whereClause.Append(" AND ct.is_active = @IsActive");
            parameters.Add("@IsActive", isActive.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM {TableName} ct {whereClause}";
        var total = await Connection.QueryFirstOrDefaultAsync<int>(countSql, parameters, Transaction);

        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} ct
            {whereClause}
            ORDER BY ct.created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        parameters.Add("@PageSize", pageSize);
        parameters.Add("@Offset", offset);

        // Query as dynamic to handle enum conversion manually
        var dynamicItems = await Connection.QueryAsync(sql, parameters, Transaction);
        
        var items = new List<ContractTemplate>();
        foreach (var item in dynamicItems)
        {
            items.Add(MapToContractTemplate(item));
        }

        return (items, total);
    }
    
    /// <summary>
    /// Maps a dynamic database row to ContractTemplate entity
    /// </summary>
    private static ContractTemplate MapToContractTemplate(dynamic row)
    {
        var templateType = ParseTemplateType((string)row.TemplateType);
        var defaultStatus = ParseContractStatus((string)row.DefaultStatus);
        
        // Helper to parse Guid from dynamic (can be Guid or string)
        Guid ParseGuid(dynamic value) => value is Guid guid ? guid : Guid.Parse((string)value);
        Guid? ParseNullableGuid(dynamic value) => value == null || (value is string s && string.IsNullOrWhiteSpace(s)) ? null : ParseGuid(value);
        
        var template = ContractTemplate.Create(
            clientId: ParseGuid(row.ClientId),
            name: (string)row.Name,
            code: (string)row.Code,
            templateType: templateType,
            content: (string)row.Content,
            description: (string)row.Description ?? string.Empty,
            companyId: ParseNullableGuid(row.CompanyId),
            defaultStatus: defaultStatus,
            tags: System.Text.Json.JsonSerializer.Deserialize<List<string>>((string)row.Tags ?? "[]"),
            createdBy: ParseNullableGuid(row.CreatedBy)
        );
        
        // Set additional properties via reflection (Id, timestamps, etc.)
        var type = typeof(ContractTemplate);
        
        type.GetProperty("Id")!.SetValue(template, ParseGuid(row.Id));
        type.GetProperty("Version")!.SetValue(template, (int)row.Version);
        type.GetProperty("IsActive")!.SetValue(template, (bool)row.IsActive);
        type.GetProperty("CreatedAt")!.SetValue(template, (DateTime)row.CreatedAt);
        type.GetProperty("UpdatedAt")!.SetValue(template, (DateTime)row.UpdatedAt);
        
        if (ParseNullableGuid(row.UpdatedBy).HasValue)
            type.GetProperty("UpdatedBy")!.SetValue(template, ParseNullableGuid(row.UpdatedBy));
            
        if ((bool)row.IsDeleted)
        {
            type.GetProperty("IsDeleted")!.SetValue(template, true);
            if (row.DeletedAt != null)
                type.GetProperty("DeletedAt")!.SetValue(template, (DateTime)row.DeletedAt);
        }
        
        return template;
    }
    
    /// <summary>
    /// Helper method to parse ContractStatus enum from database value
    /// </summary>
    private static ContractStatus ParseContractStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ContractStatus.Draft;
            
        // Try to find enum value by EnumMember attribute
        foreach (var field in typeof(ContractStatus).GetFields())
        {
            var attribute = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            if (attribute != null && attribute.Value == value)
            {
                return (ContractStatus)field.GetValue(null)!;
            }
        }

        // Fallback: try parse by name (case-insensitive)
        if (Enum.TryParse<ContractStatus>(value, true, out var result))
        {
            return result;
        }

        return ContractStatus.Draft;
    }

    public async Task<ContractTemplate?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} ct
            WHERE ct.id = @Id AND ct.client_id = @ClientId AND ct.is_deleted = 0";

        var row = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return row != null ? MapToContractTemplate(row) : null;
    }

    public async Task<ContractTemplate?> GetByCodeAsync(Guid clientId, string code)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} ct
            WHERE ct.client_id = @ClientId AND ct.code = @Code AND ct.is_deleted = 0
            LIMIT 1";

        var row = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            Code = code
        }, Transaction);

        return row != null ? MapToContractTemplate(row) : null;
    }

    public async Task<bool> CodeExistsAsync(Guid clientId, string code, Guid? excludeId = null)
    {
        var sql = new StringBuilder($@"
            SELECT COUNT(*) FROM {TableName} ct
            WHERE ct.client_id = @ClientId AND ct.code = @Code AND ct.is_deleted = 0");

        var parameters = new DynamicParameters();
        parameters.Add("@ClientId", clientId.ToString());
        parameters.Add("@Code", code);

        if (excludeId.HasValue)
        {
            sql.Append(" AND ct.id != @ExcludeId");
            parameters.Add("@ExcludeId", excludeId.Value.ToString());
        }

        var count = await Connection.QueryFirstOrDefaultAsync<int>(sql.ToString(), parameters, Transaction);
        return count > 0;
    }

    public async Task<IEnumerable<ContractTemplate>> GetActiveTemplatesAsync(Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} ct
            WHERE ct.client_id = @ClientId AND ct.is_active = 1 AND ct.is_deleted = 0
            ORDER BY ct.name ASC";

        return await Connection.QueryAsync<ContractTemplate>(sql, new
        {
            ClientId = clientId.ToString()
        }, Transaction);
    }

    public async Task<IEnumerable<ContractTemplate>> GetByTypeAsync(Guid clientId, ContractTemplateType templateType)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} ct
            WHERE ct.client_id = @ClientId AND ct.template_type = @TemplateType AND ct.is_deleted = 0
            ORDER BY ct.name ASC";

        return await Connection.QueryAsync<ContractTemplate>(sql, new
        {
            ClientId = clientId.ToString(),
            TemplateType = templateType.ToString()
        }, Transaction);
    }

    public async Task AddAsync(ContractTemplate template)
    {
        var sql = @"
            INSERT INTO contract_templates 
            (id, client_id, company_id, name, description, code, template_type, content, default_status, 
             tags, version, is_active, created_at, updated_at, created_by, updated_by, is_deleted)
            VALUES 
            (@Id, @ClientId, @CompanyId, @Name, @Description, @Code, @TemplateType, @Content, @DefaultStatus,
             @Tags, @Version, @IsActive, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, @IsDeleted)";

        await Connection.ExecuteAsync(sql, new
        {
            Id = template.Id.ToString(),
            ClientId = template.ClientId.ToString(),
            CompanyId = template.CompanyId?.ToString(),
            template.Name,
            template.Description,
            template.Code,
            TemplateType = template.TemplateType.ToString(),
            template.Content,
            DefaultStatus = template.DefaultStatus.ToString(),
            Tags = string.Join(",", template.Tags),
            template.Version,
            template.IsActive,
            template.CreatedAt,
            template.UpdatedAt,
            CreatedBy = template.CreatedBy?.ToString(),
            UpdatedBy = template.UpdatedBy?.ToString(),
            template.IsDeleted
        }, Transaction);
    }

    public async Task UpdateAsync(ContractTemplate template)
    {
        var sql = @"
            UPDATE contract_templates 
            SET name = @Name,
                description = @Description,
                content = @Content,
                default_status = @DefaultStatus,
                tags = @Tags,
                version = @Version,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId";

        await Connection.ExecuteAsync(sql, new
        {
            template.Name,
            template.Description,
            template.Content,
            DefaultStatus = template.DefaultStatus.ToString(),
            Tags = string.Join(",", template.Tags),
            template.Version,
            template.IsActive,
            template.UpdatedAt,
            UpdatedBy = template.UpdatedBy?.ToString(),
            Id = template.Id.ToString(),
            ClientId = template.ClientId.ToString()
        }, Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE contract_templates 
            SET is_deleted = 1, 
                deleted_at = @DeletedAt,
                updated_by = @DeletedBy,
                updated_at = @DeletedAt
            WHERE id = @Id AND client_id = @ClientId";

        await Connection.ExecuteAsync(sql, new
        {
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy?.ToString(),
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid clientId)
    {
        var sql = $"SELECT COUNT(*) FROM {TableName} WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";
        var count = await Connection.QueryFirstOrDefaultAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return count > 0;
    }
}
