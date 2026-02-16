// F3-REP-001: ContractRepository Implementation
// File: src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/Contract/ContractRepository.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using System.Data;
using System.Text;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for contracts using Dapper
/// </summary>
public class ContractRepository : IContractRepository
{
    private readonly DapperContext _context;

    public ContractRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        c.id AS Id,
        c.client_id AS ClientId,
        c.company_id AS CompanyId,
        c.title AS Title,
        c.description AS Description,
        c.contract_type AS ContractType,
        c.template_id AS TemplateId,
        c.document_path AS DocumentPath,
        c.document_size AS DocumentSize,
        c.document_hash AS DocumentHash,
        c.status AS Status,
        c.contract_date AS ContractDate,
        c.expiration_date AS ExpirationDate,
        c.external_reference AS ExternalReference,
        c.notes AS Notes,
        c.created_at AS CreatedAt,
        c.updated_at AS UpdatedAt,
        c.created_by AS CreatedBy,
        c.updated_by AS UpdatedBy,
        c.is_deleted AS IsDeleted,
        c.deleted_at AS DeletedAt";

    private const string TableName = "contracts";

    public async Task<(IEnumerable<Contract> Items, int Total)> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        Guid? companyId = null,
        string? search = null,
        string? status = null,
        string? contractType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var offset = (page - 1) * pageSize;
        
        var whereClause = new StringBuilder("WHERE c.client_id = @ClientId AND c.is_deleted = 0");
        var parameters = new DynamicParameters();
        parameters.Add("@ClientId", clientId.ToString());

        if (companyId.HasValue)
        {
            whereClause.Append(" AND c.company_id = @CompanyId");
            parameters.Add("@CompanyId", companyId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Append(" AND (c.title LIKE @Search OR c.description LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            whereClause.Append(" AND c.status = @Status");
            parameters.Add("@Status", status);
        }

        if (!string.IsNullOrWhiteSpace(contractType))
        {
            whereClause.Append(" AND c.contract_type = @ContractType");
            parameters.Add("@ContractType", contractType);
        }

        if (fromDate.HasValue)
        {
            whereClause.Append(" AND c.created_at >= @FromDate");
            parameters.Add("@FromDate", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            whereClause.Append(" AND c.created_at < @ToDate");
            parameters.Add("@ToDate", toDate.Value.AddDays(1));
        }

        var countSql = $"SELECT COUNT(*) FROM {TableName} c {whereClause}";
        var total = await Connection.QueryFirstOrDefaultAsync<int>(countSql, parameters, Transaction);

        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            {whereClause}
            ORDER BY c.created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        parameters.Add("@PageSize", pageSize);
        parameters.Add("@Offset", offset);

        var items = await Connection.QueryAsync<Contract>(sql, parameters, Transaction);

        return (items, total);
    }

    public async Task<Contract?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.id = @Id AND c.client_id = @ClientId AND c.is_deleted = 0";

        return await Connection.QueryFirstOrDefaultAsync<Contract>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);
    }

    public async Task<Contract?> GetWithDetailsAsync(Guid id, Guid clientId)
    {
        var contract = await GetByIdAsync(id, clientId);
        if (contract == null)
            return null;

        // Load parties
        var partiesSql = @"
            SELECT id, contract_id, party_type, party_name, party_email, user_id, shareholder_id,
                   signature_status, signature_date, signature_token, external_id, sequence_order,
                   created_at, updated_at, is_deleted
            FROM contract_parties
            WHERE contract_id = @ContractId AND is_deleted = 0
            ORDER BY sequence_order ASC";

        var parties = await Connection.QueryAsync<ContractParty>(partiesSql, new
        {
            ContractId = id.ToString()
        }, Transaction);

        // Use reflection to set readonly Parties property
        var partiesProperty = typeof(Contract).GetProperty(nameof(Contract.Parties));
        partiesProperty?.SetValue(contract, parties.ToList());

        // Load clauses
        var clausesSql = @"
            SELECT cc.id, cc.contract_id, cc.clause_id, cc.custom_content, cc.display_order,
                   cc.is_mandatory, cc.clause_variables, cc.notes, cc.created_at, cc.updated_at,
                   cc.created_by, cc.updated_by, cc.is_deleted
            FROM contract_clauses cc
            WHERE cc.contract_id = @ContractId AND cc.is_deleted = 0
            ORDER BY cc.display_order ASC";

        var clauses = await Connection.QueryAsync<ContractClause>(clausesSql, new
        {
            ContractId = id.ToString()
        }, Transaction);

        // Use reflection to set readonly Clauses property
        var clausesProperty = typeof(Contract).GetProperty(nameof(Contract.Clauses));
        clausesProperty?.SetValue(contract, clauses.ToList());

        return contract;
    }

    public async Task<IEnumerable<Contract>> GetByCompanyAsync(Guid clientId, Guid companyId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.company_id = @CompanyId AND c.is_deleted = 0
            ORDER BY c.created_at DESC";

        return await Connection.QueryAsync<Contract>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString()
        }, Transaction);
    }

    public async Task<IEnumerable<Contract>> GetByStatusAsync(Guid clientId, ContractStatus status)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.status = @Status AND c.is_deleted = 0
            ORDER BY c.created_at DESC";

        return await Connection.QueryAsync<Contract>(sql, new
        {
            ClientId = clientId.ToString(),
            Status = status.ToString()
        }, Transaction);
    }

    public async Task<IEnumerable<Contract>> GetExpiredContractsAsync(Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId 
                  AND c.expiration_date IS NOT NULL 
                  AND c.expiration_date < NOW()
                  AND c.is_deleted = 0
            ORDER BY c.expiration_date ASC";

        return await Connection.QueryAsync<Contract>(sql, new
        {
            ClientId = clientId.ToString()
        }, Transaction);
    }

    public async Task<IEnumerable<Contract>> GetByTemplateAsync(Guid clientId, Guid templateId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} c
            WHERE c.client_id = @ClientId AND c.template_id = @TemplateId AND c.is_deleted = 0
            ORDER BY c.created_at DESC";

        return await Connection.QueryAsync<Contract>(sql, new
        {
            ClientId = clientId.ToString(),
            TemplateId = templateId.ToString()
        }, Transaction);
    }

    public async Task AddAsync(Contract contract)
    {
        var sql = @"
            INSERT INTO contracts
            (id, client_id, company_id, title, description, contract_type, template_id, 
             document_path, document_size, document_hash, status, contract_date, expiration_date,
             external_reference, notes, created_at, updated_at, created_by, updated_by, is_deleted)
            VALUES
            (@Id, @ClientId, @CompanyId, @Title, @Description, @ContractType, @TemplateId,
             @DocumentPath, @DocumentSize, @DocumentHash, @Status, @ContractDate, @ExpirationDate,
             @ExternalReference, @Notes, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, @IsDeleted)";

        await Connection.ExecuteAsync(sql, new
        {
            Id = contract.Id.ToString(),
            ClientId = contract.ClientId.ToString(),
            CompanyId = contract.CompanyId.ToString(),
            contract.Title,
            contract.Description,
            ContractType = contract.ContractType.ToString(),
            TemplateId = contract.TemplateId?.ToString(),
            contract.DocumentPath,
            contract.DocumentSize,
            contract.DocumentHash,
            Status = contract.Status.ToString(),
            contract.ContractDate,
            contract.ExpirationDate,
            contract.ExternalReference,
            contract.Notes,
            contract.CreatedAt,
            contract.UpdatedAt,
            CreatedBy = contract.CreatedBy?.ToString(),
            UpdatedBy = contract.UpdatedBy?.ToString(),
            contract.IsDeleted
        }, Transaction);
    }

    public async Task UpdateAsync(Contract contract)
    {
        var sql = @"
            UPDATE contracts
            SET title = @Title,
                description = @Description,
                document_path = @DocumentPath,
                document_size = @DocumentSize,
                document_hash = @DocumentHash,
                status = @Status,
                contract_date = @ContractDate,
                expiration_date = @ExpirationDate,
                notes = @Notes,
                external_reference = @ExternalReference,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId";

        await Connection.ExecuteAsync(sql, new
        {
            contract.Title,
            contract.Description,
            contract.DocumentPath,
            contract.DocumentSize,
            contract.DocumentHash,
            Status = contract.Status.ToString(),
            contract.ContractDate,
            contract.ExpirationDate,
            contract.Notes,
            contract.ExternalReference,
            contract.UpdatedAt,
            UpdatedBy = contract.UpdatedBy?.ToString(),
            Id = contract.Id.ToString(),
            ClientId = contract.ClientId.ToString()
        }, Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE contracts
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
