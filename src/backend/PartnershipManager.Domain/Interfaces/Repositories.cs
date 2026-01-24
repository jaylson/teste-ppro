using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Interfaces;

// =====================================================
// REPOSITÓRIOS
// =====================================================

/// <summary>
/// Interface base para repositórios
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SoftDeleteAsync(Guid id, Guid? deletedBy = null);
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
}

/// <summary>
/// Repositório de clientes (entidade raiz - Core Module)
/// </summary>
public interface ICoreClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<Client?> GetByDocumentAsync(string document);
    Task<Client?> GetByEmailAsync(string email);
    Task<bool> DocumentExistsAsync(string document, Guid? excludeId = null);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
    Task<IEnumerable<Client>> GetActiveClientsAsync();
    Task<(IEnumerable<Client> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, string? status = null);
    Task<IEnumerable<Company>> GetClientCompaniesAsync(Guid clientId);
    Task<int> GetClientCompaniesCountAsync(Guid clientId);
    Task<int> GetClientUsersCountAsync(Guid clientId);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(Guid id);
    Task SoftDeleteAsync(Guid id, Guid? deletedBy = null);
    Task<bool> ExistsAsync(Guid id);
}

/// <summary>
/// Repositório de empresas
/// </summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<Company?> GetByCnpjAsync(string cnpj);
    Task<bool> CnpjExistsAsync(string cnpj, Guid? excludeId = null);
    Task<IEnumerable<Company>> GetActiveCompaniesAsync();
    Task<(IEnumerable<Company> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(Guid id);
    Task SoftDeleteAsync(Guid id, Guid? deletedBy = null);
    Task<bool> ExistsAsync(Guid id);
}

/// <summary>
/// Repositório de sócios/acionistas
/// </summary>
public interface IShareholderRepository
{
    Task<(IEnumerable<Shareholder> Items, int Total)> GetPagedAsync(
        Guid clientId,
        Guid? companyId,
        int page,
        int pageSize,
        string? search = null,
        string? type = null,
        string? status = null);

    Task<Shareholder?> GetByIdAsync(Guid id, Guid clientId);
    Task<Shareholder?> GetByDocumentAsync(Guid clientId, string document);
    Task<bool> DocumentExistsAsync(Guid clientId, string document, Guid? excludeId = null);
    Task AddAsync(Shareholder shareholder);
    Task UpdateAsync(Shareholder shareholder);
    Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null);
    Task<bool> ExistsAsync(Guid id, Guid clientId);
}

/// <summary>
/// Repositório de usuários
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email, Guid companyId);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email, Guid companyId, Guid? excludeId = null);
    Task<IEnumerable<User>> GetByCompanyAsync(Guid companyId);
    Task<IEnumerable<User>> GetActiveUsersByCompanyAsync(Guid companyId);
    Task<(IEnumerable<User> Items, int Total)> GetPagedByCompanyAsync(Guid companyId, int page, int pageSize, string? search = null);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task SoftDeleteAsync(Guid id, Guid? deletedBy = null);
    Task<bool> ExistsAsync(Guid id);
    Task UpdateRefreshTokenAsync(Guid userId, string? refreshToken, DateTime? expiry);
    Task UpdateLoginInfoAsync(Guid userId, bool success);
}

/// <summary>
/// Repositório de papéis de usuário
/// </summary>
public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId);
    Task AddAsync(UserRole userRole);
    Task DeactivateAsync(Guid userId, string role);
    Task<bool> ExistsAsync(Guid userId, string role);
}

/// <summary>
/// Repositório de logs de auditoria
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId, int limit = 100);
}

// =====================================================
// UNIT OF WORK
// =====================================================

/// <summary>
/// Unit of Work para transações
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ICompanyRepository Companies { get; }
    IUserRepository Users { get; }
    IUserRoleRepository UserRoles { get; }
    IAuditLogRepository AuditLogs { get; }
    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// =====================================================
// SERVIÇOS
// =====================================================

/// <summary>
/// Serviço de cache
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class;
}

/// <summary>
/// Serviço de data/hora (para facilitar testes)
/// </summary>
public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

/// <summary>
/// Serviço de usuário atual
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CompanyId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}

// =====================================================
// ENTIDADES ADICIONAIS
// =====================================================

/// <summary>
/// Log de auditoria
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AuditLog Create(
        string entityType,
        Guid entityId,
        AuditAction action,
        Guid? userId = null,
        Guid? companyId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}
