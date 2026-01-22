using Dapper;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories.Billing;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly DapperContext _context;

    public SubscriptionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT s.*, c.*, p.*
            FROM BillingSubscriptions s
            INNER JOIN BillingClients c ON s.ClientId = c.Id
            INNER JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE s.Id = @Id AND s.DeletedAt IS NULL";

        var subscriptionDict = new Dictionary<Guid, Subscription>();

        await _context.Connection.QueryAsync<Subscription, Client, Plan, Subscription>(
            sql,
            (subscription, client, plan) =>
            {
                if (!subscriptionDict.TryGetValue(subscription.Id, out var subscriptionEntry))
                {
                    subscriptionEntry = subscription;
                    subscriptionEntry.Client = client;
                    subscriptionEntry.Plan = plan;
                    subscriptionDict.Add(subscriptionEntry.Id, subscriptionEntry);
                }
                return subscriptionEntry;
            },
            new { Id = id },
            splitOn: "Id,Id"
        );

        return subscriptionDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT s.*, c.*, p.*
            FROM BillingSubscriptions s
            INNER JOIN BillingClients c ON s.ClientId = c.Id
            INNER JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE s.DeletedAt IS NULL
            ORDER BY s.CreatedAt DESC";

        var subscriptionDict = new Dictionary<Guid, Subscription>();

        await _context.Connection.QueryAsync<Subscription, Client, Plan, Subscription>(
            sql,
            (subscription, client, plan) =>
            {
                if (!subscriptionDict.TryGetValue(subscription.Id, out var subscriptionEntry))
                {
                    subscriptionEntry = subscription;
                    subscriptionEntry.Client = client;
                    subscriptionEntry.Plan = plan;
                    subscriptionDict.Add(subscriptionEntry.Id, subscriptionEntry);
                }
                return subscriptionEntry;
            },
            splitOn: "Id,Id"
        );

        return subscriptionDict.Values;
    }

    public async Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT s.*, c.*, p.*
            FROM BillingSubscriptions s
            INNER JOIN BillingClients c ON s.ClientId = c.Id
            INNER JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE s.ClientId = @ClientId AND s.DeletedAt IS NULL
            ORDER BY s.CreatedAt DESC";

        var subscriptionDict = new Dictionary<Guid, Subscription>();

        await _context.Connection.QueryAsync<Subscription, Client, Plan, Subscription>(
            sql,
            (subscription, client, plan) =>
            {
                if (!subscriptionDict.TryGetValue(subscription.Id, out var subscriptionEntry))
                {
                    subscriptionEntry = subscription;
                    subscriptionEntry.Client = client;
                    subscriptionEntry.Plan = plan;
                    subscriptionDict.Add(subscriptionEntry.Id, subscriptionEntry);
                }
                return subscriptionEntry;
            },
            new { ClientId = clientId },
            splitOn: "Id,Id"
        );

        return subscriptionDict.Values;
    }

    public async Task<Guid> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.Id = Guid.NewGuid();
        subscription.CreatedAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO BillingSubscriptions (Id, ClientId, PlanId, Status, StartDate, EndDate, AutoRenew, CompaniesCount, UsersCount, DueDay, PaymentMethod, CreatedAt, UpdatedAt)
            VALUES (@Id, @ClientId, @PlanId, @Status, @StartDate, @EndDate, @AutoRenew, @CompaniesCount, @UsersCount, @DueDay, @PaymentMethod, @CreatedAt, @UpdatedAt)";

        await _context.Connection.ExecuteAsync(sql, subscription);
        return subscription.Id;
    }

    public async Task<bool> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE BillingSubscriptions
            SET ClientId = @ClientId,
                PlanId = @PlanId,
                Status = @Status,
                StartDate = @StartDate,
                EndDate = @EndDate,
                AutoRenew = @AutoRenew,
                CompaniesCount = @CompaniesCount,
                UsersCount = @UsersCount,
                DueDay = @DueDay,
                PaymentMethod = @PaymentMethod,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rows = await _context.Connection.ExecuteAsync(sql, subscription);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE BillingSubscriptions
            SET DeletedAt = @DeletedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rows = await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return rows > 0;
    }
}
