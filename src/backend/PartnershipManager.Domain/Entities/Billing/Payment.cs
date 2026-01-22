namespace PartnershipManager.Domain.Entities.Billing;

/// <summary>
/// Pagamento registrado manualmente para uma fatura
/// </summary>
public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string Reference { get; set; } = string.Empty; // Número da transferência, comprovante, etc
    public string? Notes { get; set; }
    
    // Relacionamentos
    public Invoice Invoice { get; set; } = null!;
}

public enum PaymentMethod
{
    BankTransfer = 1,
    CreditCard = 2,
    Pix = 3,
    Boleto = 4,
    Cash = 5,
    Other = 99
}
