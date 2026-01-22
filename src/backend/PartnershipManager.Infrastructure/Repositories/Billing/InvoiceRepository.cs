using Dapper;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories.Billing;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly DapperContext _context;

    public InvoiceRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                i.Id, i.ClientId, i.SubscriptionId, i.InvoiceNumber, i.Amount, 
                i.IssueDate, i.DueDate, i.Status, i.PaymentDate, i.Description, 
                i.Notes, i.CreatedAt, i.CreatedBy, i.UpdatedAt, i.UpdatedBy, i.DeletedAt,
                c.Id, c.Name, c.Email, c.Document, c.Type, c.Status, c.Phone, 
                c.Address, c.City, c.State, c.ZipCode, c.Country, c.CreatedAt, 
                c.CreatedBy, c.UpdatedAt, c.UpdatedBy, c.DeletedAt,
                s.Id, s.ClientId, s.PlanId, s.StartDate, s.EndDate, s.Status, 
                s.AutoRenew, s.CreatedAt, s.CreatedBy, s.UpdatedAt, s.UpdatedBy, s.DeletedAt,
                p.Id, p.Name, p.Description, p.Price, p.BillingCycle, p.Features, 
                p.MaxCompanies, p.MaxUsers, p.IsActive, p.CreatedAt, p.CreatedBy, 
                p.UpdatedAt, p.UpdatedBy, p.DeletedAt
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE i.Id = @Id AND i.DeletedAt IS NULL";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id"
        );

        return invoiceDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT i.*, c.*, s.*, p.*
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE i.DeletedAt IS NULL
            ORDER BY i.IssueDate DESC";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            splitOn: "Id,Id,Id"
        );

        return invoiceDict.Values;
    }

    public async Task<IEnumerable<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT i.*, c.*, s.*, p.*
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE i.ClientId = @ClientId AND i.DeletedAt IS NULL
            ORDER BY i.IssueDate DESC";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            new { ClientId = clientId },
            splitOn: "Id,Id,Id"
        );

        return invoiceDict.Values;
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT i.*, c.*, s.*, p.*
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE i.Status = @Status AND i.DeletedAt IS NULL
            ORDER BY i.IssueDate DESC";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            new { Status = status },
            splitOn: "Id,Id,Id"
        );

        return invoiceDict.Values;
    }

    public async Task<IEnumerable<Invoice>> GetByFilterAsync(
        Guid? clientId = null,
        Guid? subscriptionId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? planName = null,
        CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string> { "i.DeletedAt IS NULL" };
        var parameters = new DynamicParameters();

        if (clientId.HasValue)
        {
            whereClauses.Add("i.ClientId = @ClientId");
            parameters.Add("ClientId", clientId.Value);
        }

        if (subscriptionId.HasValue)
        {
            whereClauses.Add("i.SubscriptionId = @SubscriptionId");
            parameters.Add("SubscriptionId", subscriptionId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("i.Status = @Status");
            parameters.Add("Status", status);
        }

        if (startDate.HasValue)
        {
            whereClauses.Add("i.IssueDate >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            whereClauses.Add("i.IssueDate <= @EndDate");
            parameters.Add("EndDate", endDate.Value);
        }

        if (!string.IsNullOrEmpty(planName))
        {
            whereClauses.Add("p.Name LIKE @PlanName");
            parameters.Add("PlanName", $"%{planName}%");
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT i.*, c.*, s.*, p.*
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE {whereClause}
            ORDER BY i.IssueDate DESC";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            parameters,
            splitOn: "Id,Id,Id"
        );

        return invoiceDict.Values;
    }

    public async Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(((int)InvoiceStatus.Pending).ToString(), cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT i.*, c.*, s.*, p.*
            FROM BillingInvoices i
            INNER JOIN BillingClients c ON i.ClientId = c.Id
            LEFT JOIN BillingSubscriptions s ON i.SubscriptionId = s.Id
            LEFT JOIN BillingPlans p ON s.PlanId = p.Id
            WHERE i.DueDate < @Now 
                AND i.Status = @PendingStatus 
                AND i.DeletedAt IS NULL
            ORDER BY i.DueDate ASC";

        var invoiceDict = new Dictionary<Guid, Invoice>();

        await _context.Connection.QueryAsync<Invoice, Client, Subscription?, Plan?, Invoice>(
            sql,
            (invoice, client, subscription, plan) =>
            {
                if (!invoiceDict.TryGetValue(invoice.Id, out var invoiceEntry))
                {
                    invoiceEntry = invoice;
                    invoiceEntry.Client = client;
                    
                    if (subscription != null)
                    {
                        invoiceEntry.Subscription = subscription;
                        if (plan != null)
                        {
                            subscription.Plan = plan;
                        }
                    }
                    
                    invoiceDict.Add(invoiceEntry.Id, invoiceEntry);
                }
                return invoiceEntry;
            },
            new { Now = DateTime.UtcNow, PendingStatus = (int)InvoiceStatus.Pending },
            splitOn: "Id,Id,Id"
        );

        return invoiceDict.Values;
    }

    public async Task<Guid> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO BillingInvoices 
                (Id, ClientId, SubscriptionId, InvoiceNumber, Amount, IssueDate, DueDate, Status, Description, Notes, CreatedAt, UpdatedAt)
            VALUES 
                (@Id, @ClientId, @SubscriptionId, @InvoiceNumber, @Amount, @IssueDate, @DueDate, @Status, @Description, @Notes, @CreatedAt, @UpdatedAt)";

        invoice.Id = Guid.NewGuid();
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.Connection.ExecuteAsync(sql, invoice);
        return invoice.Id;
    }

    public async Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE BillingInvoices 
            SET 
                Amount = @Amount,
                DueDate = @DueDate,
                Status = @Status,
                PaymentDate = @PaymentDate,
                Description = @Description,
                Notes = @Notes,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        invoice.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await _context.Connection.ExecuteAsync(sql, invoice);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE BillingInvoices 
            SET DeletedAt = @DeletedAt 
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rowsAffected = await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*) + 1 
            FROM BillingInvoices 
            WHERE YEAR(CreatedAt) = @Year";

        var year = DateTime.UtcNow.Year;
        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new { Year = year });
        
        return $"INV-{year}-{count:D6}";
    }
}
