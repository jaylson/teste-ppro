using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Notification;
using PartnershipManager.Application.Services;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var result = await _service.GetByUserAsync(userId, companyId, page, pageSize);
            return Ok(ApiResponse<PagedResult<NotificationResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações");
            return StatusCode(500, new { message = "Erro ao buscar notificações" });
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var count = await _service.GetUnreadCountAsync(userId, companyId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contagem de não lidas");
            return StatusCode(500, new { message = "Erro ao buscar contagem" });
        }
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent()
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var result = await _service.GetRecentAsync(userId, companyId, 10);
            return Ok(ApiResponse<IEnumerable<NotificationResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações recentes");
            return StatusCode(500, new { message = "Erro ao buscar notificações recentes" });
        }
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetRequiredUserId();
            await _service.MarkAsReadAsync(id, userId);
            return Ok(ApiResponse.Ok("Notificação marcada como lida"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação como lida {Id}", id);
            return StatusCode(500, new { message = "Erro ao marcar notificação" });
        }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            await _service.MarkAllAsReadAsync(userId, companyId);
            return Ok(ApiResponse.Ok("Todas as notificações marcadas como lidas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar todas como lidas");
            return StatusCode(500, new { message = "Erro ao marcar notificações" });
        }
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetRequiredUserId();
            var result = await _service.GetPreferencesAsync(userId);
            return Ok(ApiResponse<IEnumerable<NotificationPreferenceResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar preferências");
            return StatusCode(500, new { message = "Erro ao buscar preferências" });
        }
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreference([FromBody] UpdatePreferenceRequest request)
    {
        try
        {
            var userId = GetRequiredUserId();
            await _service.UpdatePreferenceAsync(userId, request.NotificationType, request.Channel);
            return Ok(ApiResponse.Ok("Preferência atualizada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar preferência");
            return StatusCode(500, new { message = "Erro ao atualizar preferência" });
        }
    }

    private Guid GetRequiredUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(value, out var id))
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        return id;
    }
}
