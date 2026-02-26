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
/// Gerenciamento de marcos (milestones) de vesting baseado em eventos
/// </summary>
[ApiController]
[Route("api/milestones")]
[Authorize]
[Produces("application/json")]
public class MilestonesController : ControllerBase
{
    private readonly IVestingMilestoneService _milestoneService;
    private readonly ILogger<MilestonesController> _logger;

    public MilestonesController(IVestingMilestoneService milestoneService, ILogger<MilestonesController> logger)
    {
        _milestoneService = milestoneService;
        _logger = logger;
    }

    /// <summary>
    /// Lista milestones com paginação e filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<VestingMilestoneListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? vestingPlanId,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _milestoneService.GetPagedAsync(clientId, companyId, page, pageSize, vestingPlanId, status);
        return Ok(ApiResponse<VestingMilestoneListResponse>.Ok(result));
    }

    /// <summary>
    /// Lista todos os milestones de um plano de vesting
    /// </summary>
    [HttpGet("by-plan/{vestingPlanId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VestingMilestoneResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPlan(Guid vestingPlanId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _milestoneService.GetByPlanAsync(clientId, vestingPlanId);
        return Ok(ApiResponse<IEnumerable<VestingMilestoneResponse>>.Ok(result));
    }

    /// <summary>
    /// Obtém milestone por id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VestingMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _milestoneService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<VestingMilestoneResponse>.Ok(result));
    }

    /// <summary>
    /// Cria novo milestone em um plano de vesting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VestingMilestoneResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromQuery] Guid companyId, [FromBody] CreateVestingMilestoneRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _milestoneService.CreateAsync(clientId, companyId, request, userId);
        _logger.LogInformation("Milestone criado: {MilestoneId}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<VestingMilestoneResponse>.Ok(result, "Milestone criado com sucesso"));
    }

    /// <summary>
    /// Marca um milestone como atingido
    /// </summary>
    [HttpPatch("{id:guid}/achieve")]
    [ProducesResponseType(typeof(ApiResponse<VestingMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Achieve(Guid id, [FromBody] AchieveMilestoneRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _milestoneService.AchieveAsync(id, clientId, request, userId);
        _logger.LogInformation("Milestone atingido: {MilestoneId}", id);
        return Ok(ApiResponse<VestingMilestoneResponse>.Ok(result, "Milestone marcado como atingido"));
    }

    /// <summary>
    /// Marca um milestone como falhou / não atingido
    /// </summary>
    [HttpPatch("{id:guid}/fail")]
    [ProducesResponseType(typeof(ApiResponse<VestingMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _milestoneService.MarkAsFailedAsync(id, clientId, userId);
        _logger.LogInformation("Milestone marcado como falhou: {MilestoneId}", id);
        return Ok(ApiResponse<VestingMilestoneResponse>.Ok(result, "Milestone marcado como não atingido"));
    }

    /// <summary>
    /// Remove (soft delete) um milestone
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _milestoneService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Milestone removido: {MilestoneId}", id);
        return Ok(ApiResponse.Ok("Milestone removido com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
