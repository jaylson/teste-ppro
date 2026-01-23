using System.Data;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de papéis de usuário
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly DapperContext _context;
    
    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        id AS Id,
        user_id AS UserId,
        role AS Role,
        permissions AS Permissions,
        granted_by AS GrantedBy,
        granted_at AS GrantedAt,
        expires_at AS ExpiresAt,
        is_active AS IsActive,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt";

    public UserRoleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        var sql = $@"SELECT {SelectColumns} 
                     FROM user_roles 
                     WHERE user_id = @UserId 
                       AND is_active = 1 
                       AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        return await Connection.QueryAsync<UserRole>(sql, 
            new { UserId = userId.ToString() }, Transaction);
    }

    public async Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId)
    {
        var sql = @"SELECT role 
                    FROM user_roles 
                    WHERE user_id = @UserId 
                      AND is_active = 1 
                      AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        return await Connection.QueryAsync<string>(sql, 
            new { UserId = userId.ToString() }, Transaction);
    }

    public async Task AddAsync(UserRole userRole)
    {
        var sql = @"INSERT INTO user_roles 
                    (id, user_id, role, permissions, granted_by, granted_at, expires_at, is_active, created_at, updated_at)
                    VALUES 
                    (@Id, @UserId, @Role, @Permissions, @GrantedBy, @GrantedAt, @ExpiresAt, @IsActive, @CreatedAt, @UpdatedAt)";
        
        await Connection.ExecuteAsync(sql, new
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
        }, Transaction);
    }

    public async Task DeactivateAsync(Guid userId, string role)
    {
        var sql = @"UPDATE user_roles 
                    SET is_active = 0, updated_at = UTC_TIMESTAMP() 
                    WHERE user_id = @UserId AND role = @Role AND is_active = 1";
        
        await Connection.ExecuteAsync(sql, 
            new { UserId = userId.ToString(), Role = role }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid userId, string role)
    {
        var sql = @"SELECT COUNT(1) FROM user_roles 
                    WHERE user_id = @UserId 
                      AND role = @Role 
                      AND is_active = 1 
                      AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        var count = await Connection.ExecuteScalarAsync<int>(sql, 
            new { UserId = userId.ToString(), Role = role }, Transaction);
        
        return count > 0;
    }
}
