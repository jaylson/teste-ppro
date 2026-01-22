using MediatR;
using PartnershipManager.Application.Features.Billing.Commands;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;

namespace PartnershipManager.Application.Features.Billing.Handlers;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public CreateInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken);

        var invoice = new Invoice
        {
            ClientId = request.ClientId,
            SubscriptionId = request.SubscriptionId,
            InvoiceNumber = invoiceNumber,
            Amount = request.Amount,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Status = InvoiceStatus.Pending,
            Description = request.Description,
            Notes = request.Notes
        };

        return await _invoiceRepository.CreateAsync(invoice, cancellationToken);
    }
}

public class UpdateInvoiceHandler : IRequestHandler<UpdateInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public UpdateInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (invoice == null)
            return false;

        invoice.Amount = request.Amount;
        invoice.DueDate = request.DueDate;
        invoice.Status = Enum.Parse<InvoiceStatus>(request.Status);
        invoice.Description = request.Description;
        invoice.Notes = request.Notes;

        return await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
    }
}

public class DeleteInvoiceHandler : IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        return await _invoiceRepository.DeleteAsync(request.Id, cancellationToken);
    }
}

public class MarkInvoiceAsPaidHandler : IRequestHandler<MarkInvoiceAsPaidCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public MarkInvoiceAsPaidHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (invoice == null)
            return false;

        invoice.MarkAsPaid(request.PaymentDate);
        return await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
    }
}

public class MarkInvoiceAsOverdueHandler : IRequestHandler<MarkInvoiceAsOverdueCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public MarkInvoiceAsOverdueHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> Handle(MarkInvoiceAsOverdueCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (invoice == null)
            return false;

        invoice.MarkAsOverdue();
        return await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
    }
}

public class CancelInvoiceHandler : IRequestHandler<CancelInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public CancelInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<bool> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (invoice == null)
            return false;

        invoice.Cancel();
        return await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
    }
}

public class GenerateMonthlyInvoicesHandler : IRequestHandler<GenerateMonthlyInvoicesCommand, int>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GenerateMonthlyInvoicesHandler(
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<int> Handle(GenerateMonthlyInvoicesCommand request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
        var activeSubscriptions = subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
        
        var invoicesCreated = 0;

        foreach (var subscription in activeSubscriptions)
        {
            var issueDate = request.ReferenceDate;
            var dueDate = issueDate.AddDays(30); // 30 dias para pagamento

            var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken);

            var invoice = new Invoice
            {
                ClientId = subscription.ClientId,
                SubscriptionId = subscription.Id,
                InvoiceNumber = invoiceNumber,
                Amount = subscription.Plan.Price,
                IssueDate = issueDate,
                DueDate = dueDate,
                Status = InvoiceStatus.Pending,
                Description = $"Assinatura {subscription.Plan.Name} - {issueDate:MMMM/yyyy}",
                Notes = $"Plano: {subscription.Plan.Name}\nEmpresasa: {subscription.CompaniesCount}/{subscription.Plan.MaxCompanies}\nUsuários: {subscription.UsersCount}/{subscription.Plan.MaxUsers}"
            };

            await _invoiceRepository.CreateAsync(invoice, cancellationToken);
            invoicesCreated++;
        }

        return invoicesCreated;
    }
}
