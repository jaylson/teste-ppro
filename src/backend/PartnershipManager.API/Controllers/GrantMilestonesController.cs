using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Gerenciamento de milestones de performance vinculados a grants de vesting
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public class GrantMilestonesController : ControllerBase
{
    private readonly IMilestoneTrackingService _trackingService;
    private readonly ILogger<GrantMilestonesController> _logger;

    public GrantMilestonesController(
        IMilestoneTrackingService trackingService,
        ILogger<GrantMilestonesController> logger)
    {
        _trackingService = trackingService;
        _logger = logger;
    }

    // ─── Por Grant ──────────────────────────────────────────────────────────

    /// <summary>
    /// Lista todos os milestones de um grant específico
    /// </summary>
    [HttpGet("api/grants/{grantId:guid}/milestones")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GrantMilestoneResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGrant(Guid grantId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _trackingService.GetByGrantAsync(clientId, grantId);
        return Ok(ApiResponse<IEnumerable<GrantMilestoneResponse>>.Ok(result));
    }

    /// <summary>
    /// Dashboard de milestones de um grant (resumo de progresso)
    /// </summary>
    [HttpGet("api/grants/{grantId:guid}/milestones/dashboard")]
    [ProducesResponseType(typeof(ApiResponse<MilestoneProgressDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(Guid grantId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _trackingService.GetDashboardAsync(clientId, grantId);
        return Ok(ApiResponse<MilestoneProgressDashboardResponse>.Ok(result));
    }

    // ─── CRUD de Grant Milestones ────────────────────────────────────────────

    /// <summary>
    /// Lista grant milestones com paginação e filtros
    /// </summary>
    [HttpGet("api/grant-milestones")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? grantId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _trackingService.GetPagedAsync(clientId, companyId, page, pageSize, grantId, status, category);
        return Ok(ApiResponse<GrantMilestoneListResponse>.Ok(result));
    }

    /// <summary>
    /// Obtém grant milestone por id
    /// </summary>
    [HttpGet("api/grant-milestones/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _trackingService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result));
    }

    /// <summary>
    /// Cria novo grant milestone vinculado a um grant de vesting
    /// </summary>
    [HttpPost("api/grant-milestones")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromQuery] Guid companyId, [FromBody] CreateGrantMilestoneRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _trackingService.CreateAsync(clientId, companyId, request, userId);
        _logger.LogInformation("Grant milestone criado: {MilestoneId} para grant {GrantId}", result.Id, result.VestingGrantId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<GrantMilestoneResponse>.Ok(result, "Milestone criado com sucesso"));
    }

    // ─── Progress ────────────────────────────────────────────────────────────

    /// <summary>
    /// Registra novo progresso para um milestone
    /// </summary>
    [HttpPost("api/grant-milestones/{id:guid}/progress")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordProgress(Guid id, [FromBody] RecordMilestoneProgressRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _trackingService.RecordProgressAsync(id, clientId, request, userId);
        _logger.LogInformation("Progresso registrado para milestone {MilestoneId}: {Value}", id, request.RecordedValue);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result, "Progresso registrado com sucesso"));
    }

    /// <summary>
    /// Obtém histórico de progresso de um milestone
    /// </summary>
    [HttpGet("api/grant-milestones/{id:guid}/progress")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MilestoneProgressResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgressHistory(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _trackingService.GetProgressHistoryAsync(clientId, id);
        return Ok(ApiResponse<IEnumerable<MilestoneProgressResponse>>.Ok(result));
    }

    /// <summary>
    /// Obtém série temporal de progresso para gráficos
    /// </summary>
    [HttpGet("api/grant-milestones/{id:guid}/progress/timeline")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MilestoneProgressResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgressTimeline(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var fromDate = from ?? DateTime.UtcNow.AddYears(-1);
        var toDate = to ?? DateTime.UtcNow;
        var result = await _trackingService.GetProgressTimeSeriesAsync(clientId, id, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<MilestoneProgressResponse>>.Ok(result));
    }

    // ─── Ações de Status ─────────────────────────────────────────────────────

    /// <summary>
    /// Marca milestone como atingido (achieved)
    /// </summary>
    [HttpPatch("api/grant-milestones/{id:guid}/achieve")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Achieve(Guid id, [FromBody] AchieveGrantMilestoneRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _trackingService.MarkAsAchievedAsync(id, clientId, request, userId);
        _logger.LogInformation("Grant milestone marcado como atingido: {MilestoneId}", id);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result, "Milestone marcado como atingido"));
    }

    /// <summary>
    /// Verifica/confirma que um milestone foi atingido
    /// </summary>
    [HttpPatch("api/grant-milestones/{id:guid}/verify")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyGrantMilestoneRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _trackingService.VerifyAsync(id, clientId, userId);
        _logger.LogInformation("Grant milestone verificado: {MilestoneId}", id);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result, "Milestone verificado com sucesso"));
    }

    /// <summary>
    /// Marca milestone como falhado / não atingido
    /// </summary>
    [HttpPatch("api/grant-milestones/{id:guid}/fail")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _trackingService.MarkAsFailedAsync(id, clientId, userId);
        _logger.LogInformation("Grant milestone marcado como falhado: {MilestoneId}", id);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result, "Milestone marcado como não atingido"));
    }

    /// <summary>
    /// Cancela um milestone
    /// </summary>
    [HttpPatch("api/grant-milestones/{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<GrantMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _trackingService.CancelAsync(id, clientId, userId);
        _logger.LogInformation("Grant milestone cancelado: {MilestoneId}", id);
        return Ok(ApiResponse<GrantMilestoneResponse>.Ok(result, "Milestone cancelado"));
    }

    /// <summary>
    /// Remove (soft delete) um grant milestone
    /// </summary>
    [HttpDelete("api/grant-milestones/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _trackingService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Grant milestone removido: {MilestoneId}", id);
        return Ok(ApiResponse.Ok("Milestone removido com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
