using Dapper;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories.Billing;

public class ClientRepository : IClientRepository
{
    private readonly DapperContext _context;

    public ClientRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                Id, Name, Email, Document, Type, Status, 
                Phone, Address, City, State, ZipCode, Country,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
            FROM BillingClients
            WHERE Id = @Id AND DeletedAt IS NULL";
        
        return await _context.Connection.QueryFirstOrDefaultAsync<Client>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                Id, Name, Email, Document, Type, Status, 
                Phone, Address, City, State, ZipCode, Country,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
            FROM BillingClients
            WHERE DeletedAt IS NULL
            ORDER BY CreatedAt DESC";
        
        return await _context.Connection.QueryAsync<Client>(sql);
    }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                Id, Name, Email, Document, Type, Status, 
                Phone, Address, City, State, ZipCode, Country,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
            FROM BillingClients
            WHERE Email = @Email AND DeletedAt IS NULL";
        
        return await _context.Connection.QueryFirstOrDefaultAsync<Client>(sql, new { Email = email });
    }

    public async Task<Client?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                Id, Name, Email, Document, Type, Status, 
                Phone, Address, City, State, ZipCode, Country,
                CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
            FROM BillingClients
            WHERE Document = @Document AND DeletedAt IS NULL";
        
        return await _context.Connection.QueryFirstOrDefaultAsync<Client>(sql, new { Document = document });
    }

    public async Task<Guid> CreateAsync(Client client, CancellationToken cancellationToken = default)
    {
        client.Id = Guid.NewGuid();
        client.CreatedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;

        var sql = @"
            INSERT INTO BillingClients 
                (Id, Name, Email, Document, Type, Status, Phone, Address, 
                 City, State, ZipCode, Country, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
            VALUES 
                (@Id, @Name, @Email, @Document, @Type, @Status, @Phone, @Address,
                 @City, @State, @ZipCode, @Country, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)";

        await _context.Connection.ExecuteAsync(sql, client);
        return client.Id;
    }

    public async Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        client.UpdatedAt = DateTime.UtcNow;

        var sql = @"
            UPDATE BillingClients 
            SET 
                Name = @Name,
                Email = @Email,
                Document = @Document,
                Type = @Type,
                Status = @Status,
                Phone = @Phone,
                Address = @Address,
                City = @City,
                State = @State,
                ZipCode = @ZipCode,
                Country = @Country,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rowsAffected = await _context.Connection.ExecuteAsync(sql, client);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = @"
            UPDATE BillingClients 
            SET DeletedAt = @DeletedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rowsAffected = await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<int> GetSubscriptionsCountAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(*) 
            FROM BillingSubscriptions 
            WHERE ClientId = @ClientId AND DeletedAt IS NULL";
        
        return await _context.Connection.ExecuteScalarAsync<int>(sql, new { ClientId = clientId });
    }
}
