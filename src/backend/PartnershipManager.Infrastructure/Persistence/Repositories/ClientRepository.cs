using System.Data;
using System.Text;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de clientes (entidade raiz do multi-tenancy - Core Module)
/// </summary>
public class CoreClientRepository : ICoreClientRepository
{
    private readonly DapperContext _context;
    private const string TableName = "clients";

    public CoreClientRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        var sql = @"SELECT 
                        id AS Id,
                        name AS Name,
                        trading_name AS TradingName,
                        document AS Document,
                        document_type AS DocumentType,
                        email AS Email,
                        phone AS Phone,
                        logo_url AS LogoUrl,
                        settings AS Settings,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        created_by AS CreatedBy,
                        updated_by AS UpdatedBy,
                        is_deleted AS IsDeleted,
                        deleted_at AS DeletedAt
                    FROM clients 
                    WHERE id = @Id AND is_deleted = 0";
        
        var client = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() }, Transaction);
        
        if (client == null)
            return null;

        return MapToClient(client);
    }

    public async Task<Client?> GetByDocumentAsync(string document)
    {
        // Normalizar documento (remover pontos, traços, etc)
        var normalized = new string(document.Where(char.IsDigit).ToArray());
        
        var sql = @"SELECT 
                        id AS Id,
                        name AS Name,
                        trading_name AS TradingName,
                        document AS Document,
                        document_type AS DocumentType,
                        email AS Email,
                        phone AS Phone,
                        logo_url AS LogoUrl,
                        settings AS Settings,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        created_by AS CreatedBy,
                        updated_by AS UpdatedBy,
                        is_deleted AS IsDeleted,
                        deleted_at AS DeletedAt
                    FROM clients 
                    WHERE document = @Document AND is_deleted = 0";
        
        var client = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Document = normalized }, Transaction);
        
        if (client == null)
            return null;

        return MapToClient(client);
    }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        var sql = @"SELECT 
                        id AS Id,
                        name AS Name,
                        trading_name AS TradingName,
                        document AS Document,
                        document_type AS DocumentType,
                        email AS Email,
                        phone AS Phone,
                        logo_url AS LogoUrl,
                        settings AS Settings,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        created_by AS CreatedBy,
                        updated_by AS UpdatedBy,
                        is_deleted AS IsDeleted,
                        deleted_at AS DeletedAt
                    FROM clients 
                    WHERE LOWER(email) = LOWER(@Email) AND is_deleted = 0";
        
        var client = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Email = email }, Transaction);
        
        if (client == null)
            return null;

        return MapToClient(client);
    }

    public async Task<bool> DocumentExistsAsync(string document, Guid? excludeId = null)
    {
        var normalized = new string(document.Where(char.IsDigit).ToArray());
        
        var sql = @"SELECT COUNT(1) FROM clients 
                    WHERE document = @Document 
                      AND is_deleted = 0
                      AND (@ExcludeId IS NULL OR id != @ExcludeId)";
        
        var count = await Connection.ExecuteScalarAsync<int>(sql, 
            new { Document = normalized, ExcludeId = excludeId?.ToString() }, 
            Transaction);
        
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        var sql = @"SELECT COUNT(1) FROM clients 
                    WHERE LOWER(email) = LOWER(@Email)
                      AND is_deleted = 0
                      AND (@ExcludeId IS NULL OR id != @ExcludeId)";
        
        var count = await Connection.ExecuteScalarAsync<int>(sql, 
            new { Email = email, ExcludeId = excludeId?.ToString() }, 
            Transaction);
        
        return count > 0;
    }

    public async Task<IEnumerable<Client>> GetActiveClientsAsync()
    {
        var sql = @"SELECT 
                        id AS Id,
                        name AS Name,
                        trading_name AS TradingName,
                        document AS Document,
                        document_type AS DocumentType,
                        email AS Email,
                        phone AS Phone,
                        logo_url AS LogoUrl,
                        settings AS Settings,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM clients 
                    WHERE status = 'Active' AND is_deleted = 0
                    ORDER BY name";
        
        var clients = await Connection.QueryAsync<dynamic>(sql, transaction: Transaction);
        
        return clients.Select(MapToClient);
    }

    public async Task<(IEnumerable<Client> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? search = null, 
        string? status = null)
    {
        var whereClause = new StringBuilder("WHERE is_deleted = 0");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Append(" AND (LOWER(name) LIKE @Search OR LOWER(trading_name) LIKE @Search OR LOWER(email) LIKE @Search OR document LIKE @Search)");
            parameters.Add("Search", $"%{search.ToLower()}%");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            whereClause.Append(" AND status = @Status");
            parameters.Add("Status", status);
        }

        // Contagem total
        var countSql = $"SELECT COUNT(*) FROM clients {whereClause}";
        var total = await Connection.ExecuteScalarAsync<int>(countSql, parameters, Transaction);

        // Dados paginados
        var offset = (page - 1) * pageSize;
        var dataSql = $@"SELECT 
                            id AS Id,
                            name AS Name,
                            trading_name AS TradingName,
                            document AS Document,
                            document_type AS DocumentType,
                            email AS Email,
                            phone AS Phone,
                            logo_url AS LogoUrl,
                            status AS Status,
                            created_at AS CreatedAt,
                            updated_at AS UpdatedAt
                        FROM clients 
                        {whereClause}
                        ORDER BY name
                        LIMIT @Limit OFFSET @Offset";
        
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var clients = await Connection.QueryAsync<dynamic>(dataSql, parameters, Transaction);
        
        return (clients.Select(MapToClient), total);
    }

    public async Task<IEnumerable<Company>> GetClientCompaniesAsync(Guid clientId)
    {
        var sql = @"SELECT 
                        id AS Id,
                        client_id AS ClientId,
                        name AS Name,
                        trading_name AS TradingName,
                        cnpj AS Cnpj,
                        legal_form AS LegalForm,
                        foundation_date AS FoundationDate,
                        total_shares AS TotalShares,
                        share_price AS SharePrice,
                        currency AS Currency,
                        logo_url AS LogoUrl,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM companies 
                    WHERE client_id = @ClientId AND is_deleted = 0
                    ORDER BY name";
        
        return await Connection.QueryAsync<Company>(sql, new { ClientId = clientId.ToString() }, Transaction);
    }

    public async Task<int> GetClientCompaniesCountAsync(Guid clientId)
    {
        var sql = "SELECT COUNT(*) FROM companies WHERE client_id = @ClientId AND is_deleted = 0";
        return await Connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId.ToString() }, Transaction);
    }

    public async Task<int> GetClientUsersCountAsync(Guid clientId)
    {
        var sql = "SELECT COUNT(*) FROM users WHERE client_id = @ClientId AND is_deleted = 0";
        return await Connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId.ToString() }, Transaction);
    }

    public async Task AddAsync(Client client)
    {
        var sql = @"INSERT INTO clients 
                    (id, name, trading_name, document, document_type, email, phone, logo_url, settings, status, created_at, updated_at, created_by, updated_by, is_deleted)
                    VALUES 
                    (@Id, @Name, @TradingName, @Document, @DocumentType, @Email, @Phone, @LogoUrl, @Settings, @Status, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, @IsDeleted)";
        
        await Connection.ExecuteAsync(sql, new
        {
            Id = client.Id.ToString(),
            client.Name,
            client.TradingName,
            client.Document,
            DocumentType = client.DocumentType.ToString().ToLower(),
            client.Email,
            client.Phone,
            client.LogoUrl,
            client.Settings,
            Status = client.Status.ToString(),
            client.CreatedAt,
            client.UpdatedAt,
            CreatedBy = client.CreatedBy?.ToString(),
            UpdatedBy = client.UpdatedBy?.ToString(),
            client.IsDeleted
        }, Transaction);
    }

    public async Task UpdateAsync(Client client)
    {
        var sql = @"UPDATE clients 
                    SET name = @Name,
                        trading_name = @TradingName,
                        email = @Email,
                        phone = @Phone,
                        logo_url = @LogoUrl,
                        settings = @Settings,
                        status = @Status,
                        updated_at = @UpdatedAt,
                        updated_by = @UpdatedBy
                    WHERE id = @Id";
        
        await Connection.ExecuteAsync(sql, new
        {
            Id = client.Id.ToString(),
            client.Name,
            client.TradingName,
            client.Email,
            client.Phone,
            client.LogoUrl,
            client.Settings,
            Status = client.Status.ToString(),
            client.UpdatedAt,
            UpdatedBy = client.UpdatedBy?.ToString()
        }, Transaction);
    }

    public async Task DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM clients WHERE id = @Id";
        await Connection.ExecuteAsync(sql, new { Id = id.ToString() }, Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid? deletedBy = null)
    {
        var sql = @"UPDATE clients 
                    SET is_deleted = 1, 
                        deleted_at = @DeletedAt, 
                        updated_by = @DeletedBy,
                        updated_at = @UpdatedAt
                    WHERE id = @Id";
        
        await Connection.ExecuteAsync(sql, new 
        { 
            Id = id.ToString(), 
            DeletedAt = DateTime.UtcNow, 
            DeletedBy = deletedBy?.ToString(),
            UpdatedAt = DateTime.UtcNow
        }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var sql = "SELECT COUNT(1) FROM clients WHERE id = @Id AND is_deleted = 0";
        var count = await Connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() }, Transaction);
        return count > 0;
    }

    private static Client MapToClient(dynamic row)
    {
        var documentType = Enum.Parse<DocumentType>(row.DocumentType.ToString(), true);
        var status = Enum.Parse<ClientStatus>(row.Status.ToString(), true);
        
        var client = Client.Create(
            row.Name,
            row.Document,
            documentType,
            row.Email,
            row.TradingName,
            row.Phone
        );

        // Usar reflection para setar propriedades privadas
        var type = typeof(Client);
        
        type.GetProperty("Id")!.SetValue(client, Guid.Parse(row.Id.ToString()));
        type.GetProperty("LogoUrl")!.GetSetMethod(true)!.Invoke(client, new object?[] { row.LogoUrl?.ToString() });
        type.GetProperty("Settings")!.GetSetMethod(true)!.Invoke(client, new object?[] { row.Settings?.ToString() });
        type.GetProperty("Status")!.GetSetMethod(true)!.Invoke(client, new object[] { status });
        type.GetProperty("CreatedAt")!.SetValue(client, row.CreatedAt);
        type.GetProperty("UpdatedAt")!.SetValue(client, row.UpdatedAt);
        
        if (row.CreatedBy != null)
            type.GetProperty("CreatedBy")!.SetValue(client, Guid.Parse(row.CreatedBy.ToString()));
        if (row.UpdatedBy != null)
            type.GetProperty("UpdatedBy")!.SetValue(client, Guid.Parse(row.UpdatedBy.ToString()));
        if (row.IsDeleted != null)
            type.GetProperty("IsDeleted")!.SetValue(client, Convert.ToBoolean(row.IsDeleted));
        if (row.DeletedAt != null)
            type.GetProperty("DeletedAt")!.SetValue(client, row.DeletedAt);

        return client;
    }
}
