using PartnershipManager.Application.DTOs.ClickSign;

namespace PartnershipManager.Application.Interfaces;

public interface IClickSignWebhookService
{
    Task ProcessAsync(ClickSignWebhookPayload payload);
}
