namespace PartnershipManager.Application.Features.Billing.DTOs;

public record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    
    // Dados relacionados
    public string ClientName { get; init; } = string.Empty;
    public string? PlanName { get; init; }
}

public record CreateInvoiceDto
{
    public Guid ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public record UpdateInvoiceDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public record InvoiceFilterDto
{
    public Guid? ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public string? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? PlanName { get; init; }
}

public record InvoicePdfResponseDto
{
    public byte[] PdfData { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/pdf";
}
