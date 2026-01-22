using MediatR;
using PartnershipManager.Application.Features.Billing.DTOs;

namespace PartnershipManager.Application.Features.Billing.Queries;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto?>;

public record GetAllInvoicesQuery : IRequest<IEnumerable<InvoiceDto>>;

public record GetInvoicesByClientIdQuery(Guid ClientId) : IRequest<IEnumerable<InvoiceDto>>;

public record GetInvoicesByFilterQuery : IRequest<IEnumerable<InvoiceDto>>
{
    public Guid? ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public string? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? PlanName { get; init; }
}

public record GetInvoicePdfQuery(Guid InvoiceId) : IRequest<InvoicePdfResponseDto?>;

public record GetInvoiceStatisticsQuery : IRequest<InvoiceStatisticsDto>
{
    public Guid? ClientId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record GetMrrDataQuery : IRequest<MrrDataDto>
{
    public int Months { get; init; } = 12;
}

public record GetFilteredInvoicesQuery : IRequest<IEnumerable<InvoiceDto>>
{
    public Guid? ClientId { get; init; }
    public Domain.Entities.Billing.InvoiceStatus? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid? PlanId { get; init; }
}

public record GetInvoicesByClientQuery : IRequest<IEnumerable<InvoiceDto>>
{
    public Guid ClientId { get; init; }
}
