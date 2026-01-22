using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Domain.Interfaces.Billing;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Client?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetSubscriptionsCountAsync(Guid clientId, CancellationToken cancellationToken = default);
}

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Plan plan, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Plan plan, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByFilterAsync(
        Guid? clientId = null,
        Guid? subscriptionId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? planName = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
}
