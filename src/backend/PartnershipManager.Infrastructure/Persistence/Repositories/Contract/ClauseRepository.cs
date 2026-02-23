// F3-REP-001: ClauseRepository Implementation
// File: src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/Contract/ClauseRepository.cs
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
/// Repository for standardized clauses library using Dapper
/// </summary>
public class ClauseRepository : IClauseRepository
{
    private readonly DapperContext _context;

    public ClauseRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        c.id AS Id,
        c.client_id AS ClientId,
        c.name AS Name,
        c.description AS Description,
        c.code AS Code,
        c.content AS Content,
        c.clause_type AS ClauseType,
        c.is_mandatory AS IsMandatory,
        c.tags AS Tags,
        c.display_order AS DisplayOrder,
        c.version AS Version,
        c.is_active AS IsActive,
        c.created_at AS CreatedAt,
        c.updated_at AS UpdatedAt,
        c.created_by AS CreatedBy,
        c.updated_by AS UpdatedBy,
        c.is_deleted AS IsDeleted,
        c.deleted_at AS DeletedAt";

    private const string TableName = "clauses";

    public async Task<(IEnumerable<Clause> Items, int Total)> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        string? clauseType = null,
        bool? isMandatory = null,
        bool? isActive = null)
    {
        var offset = (page - 1) * pageSize;
        
        var whereClause = new StringBuilder("WHERE c.client_id = @ClientId AND c.is_deleted = 0");
        var parameters = new DynamicParameters();
        parameters.Add("@ClientId", clientId.ToString());

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Append(" AND (c.name LIKE @Search OR c.description LIKE @Search OR c.code LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (!string.IsNullOrWhiteSpace(clauseType))
        {
            whereClause.Append(" AND c.clause_type = @ClauseType");
            parameters.Add("@ClauseType", clauseType);
        }

        if (isMandatory.HasValue)
        {
            whereClause.Append(" AND c.is_mandatory = @IsMandatory");
            parameters.Add("@IsMandatory", isMandatory.Value);
        }

        if (isActive.HasValue)
        {
            whereClause.Append(" AND c.is_active = @IsActive");
            parameters.Add("@IsActive", isActive.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM {TableName} c {whereClause}";
        var total = await Connection.QueryFirstOrDefaultAsync<int>(countSql, parameters, Transaction);

        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            {whereClause}
            ORDER BY c.display_order ASC, c.name ASC
            LIMIT @PageSize OFFSET @Offset";

        parameters.Add("@PageSize", pageSize);
        parameters.Add("@Offset", offset);

        var rows = await Connection.QueryAsync(sql, parameters, Transaction);
        var items = rows.Select(MapToClause);

        return (items, total);
    }

    public async Task<Clause?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.id = @Id AND c.client_id = @ClientId AND c.is_deleted = 0";

        var row = await Connection.QueryFirstOrDefaultAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return row != null ? MapToClause(row) : null;
    }

    public async Task<Clause?> GetByCodeAsync(Guid clientId, string code)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.code = @Code AND c.is_deleted = 0
            LIMIT 1";

        var row = await Connection.QueryFirstOrDefaultAsync(sql, new
        {
            ClientId = clientId.ToString(),
            Code = code
        }, Transaction);

        return row != null ? MapToClause(row) : null;
    }

    public async Task<IEnumerable<Clause>> GetByTypeAsync(Guid clientId, ClauseType clauseType)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.clause_type = @ClauseType AND c.is_deleted = 0
            ORDER BY c.display_order ASC, c.name ASC";

        var rows = await Connection.QueryAsync(sql, new
        {
            ClientId = clientId.ToString(),
            ClauseType = clauseType.ToString()
        }, Transaction);

        return rows.Select(MapToClause);
    }

    public async Task<IEnumerable<Clause>> GetMandatoryClausesAsync(Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.is_mandatory = 1 AND c.is_deleted = 0
            ORDER BY c.display_order ASC, c.name ASC";

        var rows = await Connection.QueryAsync(sql, new
        {
            ClientId = clientId.ToString()
        }, Transaction);

        return rows.Select(MapToClause);
    }

    public async Task<IEnumerable<Clause>> GetActiveClausesAsync(Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.is_active = 1 AND c.is_deleted = 0
            ORDER BY c.display_order ASC, c.name ASC";

        var rows = await Connection.QueryAsync(sql, new
        {
            ClientId = clientId.ToString()
        }, Transaction);

        return rows.Select(MapToClause);
    }

    public async Task<bool> CodeExistsAsync(Guid clientId, string code, Guid? excludeId = null)
    {
        var sql = new StringBuilder($@"
            SELECT COUNT(*) FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.code = @Code AND c.is_deleted = 0");

        var parameters = new DynamicParameters();
        parameters.Add("@ClientId", clientId.ToString());
        parameters.Add("@Code", code);

        if (excludeId.HasValue)
        {
            sql.Append(" AND c.id != @ExcludeId");
            parameters.Add("@ExcludeId", excludeId.Value.ToString());
        }

        var count = await Connection.QueryFirstOrDefaultAsync<int>(sql.ToString(), parameters, Transaction);
        return count > 0;
    }

    public async Task AddAsync(Clause clause)
    {
        var sql = @"
            INSERT INTO clauses
            (id, client_id, name, description, code, content, clause_type, is_mandatory, 
             tags, display_order, version, is_active, created_at, updated_at, created_by, updated_by, is_deleted)
            VALUES
            (@Id, @ClientId, @Name, @Description, @Code, @Content, @ClauseType, @IsMandatory,
             @Tags, @DisplayOrder, @Version, @IsActive, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, @IsDeleted)";

        await Connection.ExecuteAsync(sql, new
        {
            Id = clause.Id.ToString(),
            ClientId = clause.ClientId.ToString(),
            clause.Name,
            clause.Description,
            clause.Code,
            clause.Content,
            ClauseType = clause.ClauseType.ToString(),
            clause.IsMandatory,
            Tags = string.Join(",", clause.Tags),
            clause.DisplayOrder,
            clause.Version,
            clause.IsActive,
            clause.CreatedAt,
            clause.UpdatedAt,
            CreatedBy = clause.CreatedBy?.ToString(),
            UpdatedBy = clause.UpdatedBy?.ToString(),
            clause.IsDeleted
        }, Transaction);
    }

    public async Task UpdateAsync(Clause clause)
    {
        var sql = @"
            UPDATE clauses
            SET name = @Name,
                description = @Description,
                content = @Content,
                is_mandatory = @IsMandatory,
                tags = @Tags,
                display_order = @DisplayOrder,
                version = @Version,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId";

        await Connection.ExecuteAsync(sql, new
        {
            clause.Name,
            clause.Description,
            clause.Content,
            clause.IsMandatory,
            Tags = string.Join(",", clause.Tags),
            clause.DisplayOrder,
            clause.Version,
            clause.IsActive,
            clause.UpdatedAt,
            UpdatedBy = clause.UpdatedBy?.ToString(),
            Id = clause.Id.ToString(),
            ClientId = clause.ClientId.ToString()
        }, Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE clauses
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

    private static Clause MapToClause(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null ? null : (value is Guid g ? g : Guid.Parse(value.ToString()!));
        
        var clauseTypeString = (string)row.ClauseType;
        var clauseType = ParseClauseType(clauseTypeString);

        var tagsString = (string)row.Tags;
        var tags = string.IsNullOrWhiteSpace(tagsString) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(tagsString) ?? new List<string>();

        var clause = Clause.Create(
            clientId: ParseGuid(row.ClientId),
            name: (string)row.Name,
            code: (string)row.Code,
            content: (string)row.Content,
            clauseType: clauseType,
            isMandatory: row.IsMandatory is bool b1 ? b1 : row.IsMandatory == 1,
            description: (string?)row.Description,
            tags: tags,
            displayOrder: (int)row.DisplayOrder,
            createdBy: ParseNullableGuid(row.CreatedBy)
        );

        clause.Id = ParseGuid(row.Id);
        clause.CreatedAt = (DateTime)row.CreatedAt;
        clause.UpdatedAt = (DateTime)row.UpdatedAt;
        clause.Version = (int)row.Version;
        clause.IsActive = row.IsActive is bool b2 ? b2 : row.IsActive == 1;
        clause.IsDeleted = row.IsDeleted is bool b3 ? b3 : row.IsDeleted == 1;
        clause.DeletedAt = row.DeletedAt as DateTime?;

        return clause;
    }

    private static ClauseType ParseClauseType(string value)
    {
        // Map database snake_case values to enum
        foreach (var field in typeof(ClauseType).GetFields().Where(f => f.IsLiteral))
        {
            var attribute = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            if (attribute != null && attribute.Value == value)
            {
                return (ClauseType)field.GetValue(null)!;
            }
        }

        // Fallback to enum name parsing
        if (Enum.TryParse<ClauseType>(value, true, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid ClauseType value: {value}");
    }
}
