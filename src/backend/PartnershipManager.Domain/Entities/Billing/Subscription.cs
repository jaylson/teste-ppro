namespace PartnershipManager.Domain.Entities.Billing;

/// <summary>
/// Assinatura de um cliente a um plano
/// </summary>
public class Subscription : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AutoRenew { get; set; }
    public int CompaniesCount { get; set; } // Uso atual
    public int UsersCount { get; set; } // Uso atual
    public int DueDay { get; set; } // Dia do vencimento da fatura (1-31)
    public PaymentMethod PaymentMethod { get; set; } // Método de pagamento preferencial
    
    // Relacionamentos
    public Client Client { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    
    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Suspend()
    {
        Status = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        EndDate = DateTime.UtcNow;
        AutoRenew = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum SubscriptionStatus
{
    Pending = 1,
    Active = 2,
    Suspended = 3,
    Cancelled = 4
}
