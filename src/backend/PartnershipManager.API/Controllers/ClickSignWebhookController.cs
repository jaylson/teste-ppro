using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.Application.DTOs.ClickSign;
using PartnershipManager.Application.Interfaces;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/clicksign/webhook")]
public class ClickSignWebhookController : ControllerBase
{
    private readonly IClickSignWebhookService _webhookService;
    private readonly ILogger<ClickSignWebhookController> _logger;

    public ClickSignWebhookController(
        IClickSignWebhookService webhookService,
        ILogger<ClickSignWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook([FromBody] ClickSignWebhookPayload payload)
    {
        try
        {
            await _webhookService.ProcessAsync(payload);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook ClickSign");
            return StatusCode(500, new { success = false, message = "Erro ao processar webhook" });
        }
    }
}
