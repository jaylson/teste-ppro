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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
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

public class GetInvoiceStatisticsHandler : IRequestHandler<GetInvoiceStatisticsQuery, InvoiceStatisticsDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceStatisticsHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceStatisticsDto> Handle(GetInvoiceStatisticsQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        
        // Aplicar filtros
        if (request.ClientId.HasValue)
            invoices = invoices.Where(i => i.ClientId == request.ClientId.Value);
            
        if (request.StartDate.HasValue)
            invoices = invoices.Where(i => i.IssueDate >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            invoices = invoices.Where(i => i.IssueDate <= request.EndDate.Value);

        var invoicesList = invoices.ToList();

        return new InvoiceStatisticsDto
        {
            TotalRevenue = invoicesList.Where(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Paid).Sum(i => i.Amount),
            PendingRevenue = invoicesList.Where(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Pending).Sum(i => i.Amount),
            OverdueRevenue = invoicesList.Where(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Overdue).Sum(i => i.Amount),
            TotalInvoices = invoicesList.Count,
            PaidInvoices = invoicesList.Count(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Paid),
            PendingInvoices = invoicesList.Count(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Pending),
            OverdueInvoices = invoicesList.Count(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Overdue),
            CancelledInvoices = invoicesList.Count(i => i.Status == Domain.Entities.Billing.InvoiceStatus.Cancelled)
        };
    }
}

public class GetFilteredInvoicesHandler : IRequestHandler<GetFilteredInvoicesQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetFilteredInvoicesHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetFilteredInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        
        // Aplicar filtros
        if (request.ClientId.HasValue)
            invoices = invoices.Where(i => i.ClientId == request.ClientId.Value);
            
        if (request.Status.HasValue)
            invoices = invoices.Where(i => i.Status == request.Status.Value);
            
        if (request.StartDate.HasValue)
            invoices = invoices.Where(i => i.IssueDate >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            invoices = invoices.Where(i => i.IssueDate <= request.EndDate.Value);

        if (request.PlanId.HasValue)
            invoices = invoices.Where(i => i.Subscription != null && i.Subscription.PlanId == request.PlanId.Value);

        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Domain.Entities.Billing.Invoice invoice)
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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
        };
    }
}

public class GetInvoicesByClientHandler : IRequestHandler<GetInvoicesByClientQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesByClientHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesByClientQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Domain.Entities.Billing.Invoice invoice)
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
            PaymentDate = invoice.PaymentDate,
            CreatedAt = invoice.CreatedAt,
            ClientName = invoice.Client?.Name ?? string.Empty,
            ClientEmail = invoice.Client?.Email ?? string.Empty,
            ClientDocument = invoice.Client?.Document ?? string.Empty,
            PlanName = invoice.Subscription?.Plan?.Name ?? string.Empty,
            ReferenceMonth = invoice.IssueDate.Month,
            ReferenceYear = invoice.IssueDate.Year
        };
    }
}

public class GetMrrDataHandler : IRequestHandler<GetMrrDataQuery, MrrDataDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetMrrDataHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<MrrDataDto> Handle(GetMrrDataQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthsToFetch = request.Months;
        var monthlyData = new List<MonthlyRevenueDto>();

        // Buscar todas as faturas
        var allInvoices = await _invoiceRepository.GetAllAsync(cancellationToken);

        // Processar últimos N meses
        for (int i = monthsToFetch - 1; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;

            // Filtrar faturas PAGAS deste mês/ano
            var monthInvoices = allInvoices.Where(inv =>
                inv.Status == Domain.Entities.Billing.InvoiceStatus.Paid &&
                inv.PaymentDate.HasValue &&
                inv.PaymentDate.Value.Year == year &&
                inv.PaymentDate.Value.Month == month
            ).ToList();

            var revenue = monthInvoices.Sum(inv => inv.Amount);
            var count = monthInvoices.Count;

            var monthName = new DateTime(year, month, 1).ToString("MMM/yyyy", new System.Globalization.CultureInfo("pt-BR"));

            monthlyData.Add(new MonthlyRevenueDto
            {
                Year = year,
                Month = month,
                MonthName = monthName,
                Revenue = revenue,
                InvoiceCount = count
            });
        }

        // Calcular métricas
        var currentMrr = monthlyData.LastOrDefault()?.Revenue ?? 0;
        var averageMrr = monthlyData.Any() ? monthlyData.Average(m => m.Revenue) : 0;
        
        // Taxa de crescimento (comparando último mês com penúltimo)
        var growthRate = 0m;
        if (monthlyData.Count >= 2)
        {
            var previousMrr = monthlyData[monthlyData.Count - 2].Revenue;
            if (previousMrr > 0)
            {
                growthRate = ((currentMrr - previousMrr) / previousMrr) * 100;
            }
        }

        return new MrrDataDto
        {
            MonthlyData = monthlyData,
            CurrentMrr = currentMrr,
            AverageMrr = averageMrr,
            GrowthRate = Math.Round(growthRate, 2)
        };
    }
}
