namespace PartnershipManager.Domain.Entities.Billing;

/// <summary>
/// Fatura gerada para uma assinatura
/// </summary>
public class Invoice : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    // Relacionamentos
    public Client Client { get; set; } = null!;
    public Subscription? Subscription { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    public void MarkAsPaid(DateTime paymentDate)
    {
        Status = InvoiceStatus.Paid;
        PaymentDate = paymentDate;
        UpdatedAt = paymentDate;
    }
    
    public void MarkAsOverdue()
    {
        if (DueDate < DateTime.UtcNow && Status == InvoiceStatus.Pending)
        {
            Status = InvoiceStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}
