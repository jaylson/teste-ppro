using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de papéis de usuários
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly DapperContext _context;

    public UserRoleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        var sql = @"SELECT 
                        id AS Id,
                        user_id AS UserId,
                        role AS Role,
                        permissions AS Permissions,
                        granted_by AS GrantedBy,
                        granted_at AS GrantedAt,
                        expires_at AS ExpiresAt,
                        is_active AS IsActive,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM user_roles 
                    WHERE user_id = @UserId AND is_active = 1";
        
        return await _context.Connection.QueryAsync<UserRole>(sql, 
            new { UserId = userId.ToString() }, _context.Transaction);
    }

    public async Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId)
    {
        var sql = @"SELECT role FROM user_roles 
                    WHERE user_id = @UserId AND is_active = 1 
                    AND (expires_at IS NULL OR expires_at > NOW())";
        
        return await _context.Connection.QueryAsync<string>(sql, 
            new { UserId = userId.ToString() }, _context.Transaction);
    }

    public async Task AddAsync(UserRole userRole)
    {
        var sql = @"INSERT INTO user_roles 
                    (id, user_id, role, permissions, granted_by, granted_at, expires_at, is_active, created_at, updated_at)
                    VALUES 
                    (@Id, @UserId, @Role, @Permissions, @GrantedBy, @GrantedAt, @ExpiresAt, @IsActive, @CreatedAt, @UpdatedAt)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = userRole.Id.ToString(),
            UserId = userRole.UserId.ToString(),
            Role = userRole.Role.ToString(),
            userRole.Permissions,
            GrantedBy = userRole.GrantedBy?.ToString(),
            userRole.GrantedAt,
            userRole.ExpiresAt,
            userRole.IsActive,
            userRole.CreatedAt,
            userRole.UpdatedAt
        }, _context.Transaction);
    }

    public async Task DeactivateAsync(Guid userId, string role)
    {
        var sql = @"UPDATE user_roles SET 
                        is_active = 0, 
                        updated_at = @UpdatedAt 
                    WHERE user_id = @UserId AND role = @Role";

        await _context.Connection.ExecuteAsync(sql, new
        {
            UserId = userId.ToString(),
            Role = role,
            UpdatedAt = DateTime.UtcNow
        }, _context.Transaction);
    }

    public async Task<bool> ExistsAsync(Guid userId, string role)
    {
        var sql = @"SELECT COUNT(1) FROM user_roles 
                    WHERE user_id = @UserId AND role = @Role AND is_active = 1";
        
        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, 
            new { UserId = userId.ToString(), Role = role }, _context.Transaction);
        return count > 0;
    }
}

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
