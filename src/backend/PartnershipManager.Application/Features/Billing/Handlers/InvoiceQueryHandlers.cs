using MediatR;
using PartnershipManager.Application.Features.Billing.DTOs;
using PartnershipManager.Application.Features.Billing.Queries;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;
using PartnershipManager.Domain.Interfaces.Services;

namespace PartnershipManager.Application.Features.Billing.Handlers;

public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        return invoice == null ? null : MapToDto(invoice);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            SubscriptionId = invoice.SubscriptionId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Description = invoice.Description,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name
        };
    }
}

public class GetAllInvoicesHandler : IRequestHandler<GetAllInvoicesQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            SubscriptionId = invoice.SubscriptionId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Description = invoice.Description,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name
        };
    }
}

public class GetInvoicesByClientIdHandler : IRequestHandler<GetInvoicesByClientIdQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesByClientIdHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesByClientIdQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            SubscriptionId = invoice.SubscriptionId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Description = invoice.Description,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name
        };
    }
}

public class GetInvoicesByFilterHandler : IRequestHandler<GetInvoicesByFilterQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesByFilterHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesByFilterQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByFilterAsync(
            request.ClientId,
            request.SubscriptionId,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.PlanName,
            cancellationToken);

        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            SubscriptionId = invoice.SubscriptionId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Description = invoice.Description,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name
        };
    }
}

public class GetInvoicePdfHandler : IRequestHandler<GetInvoicePdfQuery, InvoicePdfResponseDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public GetInvoicePdfHandler(
        IInvoiceRepository invoiceRepository,
        IPdfGeneratorService pdfGeneratorService)
    {
        _invoiceRepository = invoiceRepository;
        _pdfGeneratorService = pdfGeneratorService;
    }

    public async Task<InvoicePdfResponseDto?> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        
        if (invoice == null)
            return null;

        var pdfData = await _pdfGeneratorService.GenerateInvoicePdfAsync(invoice, cancellationToken);

        return new InvoicePdfResponseDto
        {
            PdfData = pdfData,
            FileName = $"{invoice.InvoiceNumber}.pdf",
            ContentType = "application/pdf"
        };
    }
}
