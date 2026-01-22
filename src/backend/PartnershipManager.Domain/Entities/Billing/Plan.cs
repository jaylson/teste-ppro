namespace PartnershipManager.Domain.Entities.Billing;

/// <summary>
/// Plano de assinatura disponível
/// </summary>
public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public int MaxCompanies { get; set; } // -1 = ilimitado
    public int MaxUsers { get; set; } // -1 = ilimitado
    public bool IsActive { get; set; }
    public string? Features { get; set; } // JSON com lista de features
    
    // Relacionamentos
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public enum BillingCycle
{
    Monthly = 1,
    Yearly = 2
}
