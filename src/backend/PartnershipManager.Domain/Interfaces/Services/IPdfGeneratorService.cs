using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Domain.Interfaces.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
