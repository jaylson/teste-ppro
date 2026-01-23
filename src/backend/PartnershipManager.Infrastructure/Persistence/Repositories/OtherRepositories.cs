using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de logs de auditoria (append-only)
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly DapperContext _context;

    public AuditLogRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        var sql = @"INSERT INTO audit_logs 
                    (id, company_id, user_id, action, entity_type, entity_id, 
                     old_values, new_values, ip_address, user_agent, created_at)
                    VALUES 
                    (@Id, @CompanyId, @UserId, @Action, @EntityType, @EntityId,
                     @OldValues, @NewValues, @IpAddress, @UserAgent, @CreatedAt)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = auditLog.Id.ToString(),
            CompanyId = auditLog.CompanyId?.ToString(),
            UserId = auditLog.UserId?.ToString(),
            Action = auditLog.Action.ToString(),
            auditLog.EntityType,
            EntityId = auditLog.EntityId.ToString(),
            auditLog.OldValues,
            auditLog.NewValues,
            auditLog.IpAddress,
            auditLog.UserAgent,
            auditLog.CreatedAt
        }, _context.Transaction);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, int limit = 100)
    {
        var sql = @"SELECT 
                        id AS Id,
                        company_id AS CompanyId,
                        user_id AS UserId,
                        action AS Action,
                        entity_type AS EntityType,
                        entity_id AS EntityId,
                        old_values AS OldValues,
                        new_values AS NewValues,
                        ip_address AS IpAddress,
                        user_agent AS UserAgent,
                        created_at AS CreatedAt
                    FROM audit_logs 
                    WHERE entity_type = @EntityType AND entity_id = @EntityId
                    ORDER BY created_at DESC
                    LIMIT @Limit";

        return await _context.Connection.QueryAsync<AuditLog>(sql, new
        {
            EntityType = entityType,
            EntityId = entityId.ToString(),
            Limit = limit
        }, _context.Transaction);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int limit = 100)
    {
        var sql = @"SELECT 
                        id AS Id,
                        company_id AS CompanyId,
                        user_id AS UserId,
                        action AS Action,
                        entity_type AS EntityType,
                        entity_id AS EntityId,
                        old_values AS OldValues,
                        new_values AS NewValues,
                        ip_address AS IpAddress,
                        user_agent AS UserAgent,
                        created_at AS CreatedAt
                    FROM audit_logs 
                    WHERE user_id = @UserId
                    ORDER BY created_at DESC
                    LIMIT @Limit";

        return await _context.Connection.QueryAsync<AuditLog>(sql, new
        {
            UserId = userId.ToString(),
            Limit = limit
        }, _context.Transaction);
    }

    public async Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId, int limit = 100)
    {
        var sql = @"SELECT 
                        id AS Id,
                        company_id AS CompanyId,
                        user_id AS UserId,
                        action AS Action,
                        entity_type AS EntityType,
                        entity_id AS EntityId,
                        old_values AS OldValues,
                        new_values AS NewValues,
                        ip_address AS IpAddress,
                        user_agent AS UserAgent,
                        created_at AS CreatedAt
                    FROM audit_logs 
                    WHERE company_id = @CompanyId
                    ORDER BY created_at DESC
                    LIMIT @Limit";

        return await _context.Connection.QueryAsync<AuditLog>(sql, new
        {
            CompanyId = companyId.ToString(),
            Limit = limit
        }, _context.Transaction);
    }
}

/// <summary>
/// Unit of Work para gerenciar transações
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _context;
    private ICompanyRepository? _companies;
    private IUserRepository? _users;
    private IUserRoleRepository? _userRoles;
    private IAuditLogRepository? _auditLogs;
    private bool _disposed;

    public UnitOfWork(DapperContext context)
    {
        _context = context;
    }

    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IUserRoleRepository UserRoles => _userRoles ??= new UserRoleRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public async Task BeginTransactionAsync()
    {
        await _context.BeginTransactionAsync();
    }

    public Task CommitTransactionAsync()
    {
        _context.CommitTransaction();
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync()
    {
        _context.RollbackTransaction();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
