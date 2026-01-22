namespace PartnershipManager.Domain.Entities.Billing;

/// <summary>
/// Cliente que pode ter uma ou mais assinaturas
/// </summary>
public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty; // CPF ou CNPJ
    public ClientType Type { get; set; } // Individual ou Company
    public ClientStatus Status { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    // Relacionamentos
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public enum ClientType
{
    Individual = 1,
    Company = 2
}

public enum ClientStatus
{
    Active = 1,
    Suspended = 2,
    Cancelled = 3
}
