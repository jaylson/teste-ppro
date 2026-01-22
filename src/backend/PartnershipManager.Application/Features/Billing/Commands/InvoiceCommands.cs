using MediatR;
using PartnershipManager.Application.Features.Billing.DTOs;

namespace PartnershipManager.Application.Features.Billing.Commands;

public record CreateInvoiceCommand : IRequest<Guid>
{
    public Guid ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public record UpdateInvoiceCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public record DeleteInvoiceCommand(Guid Id) : IRequest<bool>;

public record MarkInvoiceAsPaidCommand(Guid Id, DateTime PaymentDate) : IRequest<bool>;

public record MarkInvoiceAsOverdueCommand(Guid Id) : IRequest<bool>;

public record CancelInvoiceCommand(Guid Id) : IRequest<bool>;

public record GenerateMonthlyInvoicesCommand : IRequest<int>
{
    public DateTime ReferenceDate { get; init; }
}
