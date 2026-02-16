using Dapper;
using Microsoft.Extensions.Logging;
using PartnershipManager.Application.DTOs.ClickSign;
using PartnershipManager.Application.Interfaces;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Services;

public class ClickSignWebhookService : IClickSignWebhookService
{
    private readonly DapperContext _context;
    private readonly ILogger<ClickSignWebhookService> _logger;

    public ClickSignWebhookService(DapperContext context, ILogger<ClickSignWebhookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessAsync(ClickSignWebhookPayload payload)
    {
        var eventName = payload.Event?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(eventName))
        {
            _logger.LogWarning("ClickSign webhook received without event name");
            return;
        }

        var externalReference = payload.Data?.Attributes?.ExternalId ?? payload.Data?.Id;
        if (string.IsNullOrWhiteSpace(externalReference))
        {
            _logger.LogWarning("ClickSign webhook missing external reference");
            return;
        }

        var contractId = await _context.Connection.QueryFirstOrDefaultAsync<Guid?>(
            "SELECT id FROM contracts WHERE external_reference = @ExternalReference AND is_deleted = 0",
            new { ExternalReference = externalReference },
            _context.Transaction);

        if (!contractId.HasValue)
        {
            _logger.LogWarning("Contract not found for ClickSign external reference {ExternalReference}", externalReference);
            return;
        }

        switch (eventName)
        {
            case "sign":
                await HandleSignAsync(contractId.Value, payload);
                break;
            case "document_closed":
                await UpdateContractStatusAsync(contractId.Value, "Executed");
                break;
            case "cancel":
                await UpdateContractStatusAsync(contractId.Value, "Cancelled");
                break;
            case "deadline":
                await UpdateContractStatusAsync(contractId.Value, "Expired");
                break;
            case "refusal":
                await HandleRefusalAsync(contractId.Value, payload);
                break;
            default:
                _logger.LogInformation("ClickSign webhook event ignored: {Event}", eventName);
                break;
        }
    }

    private async Task HandleSignAsync(Guid contractId, ClickSignWebhookPayload payload)
    {
        var signerEmail = payload.Data?.Attributes?.SignerEmail ?? payload.Data?.Attributes?.Email;
        if (string.IsNullOrWhiteSpace(signerEmail))
        {
            _logger.LogWarning("ClickSign sign event without signer email for contract {ContractId}", contractId);
            return;
        }

        var updatePartySql = @"
            UPDATE contract_parties
            SET signature_status = 'Signed',
                signature_date = @SignatureDate,
                external_id = @ExternalId,
                updated_at = @SignatureDate
            WHERE contract_id = @ContractId AND party_email = @SignerEmail AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(updatePartySql, new
        {
            ContractId = contractId.ToString(),
            SignerEmail = signerEmail,
            ExternalId = payload.Data?.Id,
            SignatureDate = DateTime.UtcNow
        }, _context.Transaction);

        await UpdateContractSignatureProgressAsync(contractId);
    }

    private async Task HandleRefusalAsync(Guid contractId, ClickSignWebhookPayload payload)
    {
        var signerEmail = payload.Data?.Attributes?.SignerEmail ?? payload.Data?.Attributes?.Email;
        if (!string.IsNullOrWhiteSpace(signerEmail))
        {
            var updatePartySql = @"
                UPDATE contract_parties
                SET signature_status = 'Rejected',
                    updated_at = @UpdatedAt
                WHERE contract_id = @ContractId AND party_email = @SignerEmail AND is_deleted = 0";

            await _context.Connection.ExecuteAsync(updatePartySql, new
            {
                ContractId = contractId.ToString(),
                SignerEmail = signerEmail,
                UpdatedAt = DateTime.UtcNow
            }, _context.Transaction);
        }

        await UpdateContractStatusAsync(contractId, "Cancelled");
    }

    private async Task UpdateContractSignatureProgressAsync(Guid contractId)
    {
        var totals = await _context.Connection.QueryFirstAsync<(int total, int signed)>(
            @"SELECT COUNT(*) AS total,
                     SUM(CASE WHEN signature_status = 'Signed' THEN 1 ELSE 0 END) AS signed
              FROM contract_parties
              WHERE contract_id = @ContractId AND is_deleted = 0",
            new { ContractId = contractId.ToString() },
            _context.Transaction);

        var newStatus = totals.signed == 0
            ? "SentForSignature"
            : totals.signed == totals.total
                ? "Signed"
                : "PartiallySigned";

        await UpdateContractStatusAsync(contractId, newStatus);
    }

    private async Task UpdateContractStatusAsync(Guid contractId, string status)
    {
        var sql = @"
            UPDATE contracts
            SET status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @ContractId";

        await _context.Connection.ExecuteAsync(sql, new
        {
            ContractId = contractId.ToString(),
            Status = status,
            UpdatedAt = DateTime.UtcNow
        }, _context.Transaction);
    }
}
