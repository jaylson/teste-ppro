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
    public DateTime? PaymentDate { get; init; }
    
    // Dados relacionados
    public string ClientName { get; init; } = string.Empty;
    public string ClientEmail { get; init; } = string.Empty;
    public string ClientDocument { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public int ReferenceMonth { get; init; }
    public int ReferenceYear { get; init; }
}

public record InvoiceStatisticsDto
{
    public decimal TotalRevenue { get; init; }
    public decimal PendingRevenue { get; init; }
    public decimal OverdueRevenue { get; init; }
    public int TotalInvoices { get; init; }
    public int PaidInvoices { get; init; }
    public int PendingInvoices { get; init; }
    public int OverdueInvoices { get; init; }
    public int CancelledInvoices { get; init; }
}

public record MonthlyRevenueDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int InvoiceCount { get; init; }
}

public record MrrDataDto
{
    public List<MonthlyRevenueDto> MonthlyData { get; init; } = new();
    public decimal CurrentMrr { get; init; }
    public decimal AverageMrr { get; init; }
    public decimal GrowthRate { get; init; }
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
