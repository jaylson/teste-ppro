using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Aplicação e pré-visualização de acelerações de vesting por milestone de performance
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public class VestingAccelerationsController : ControllerBase
{
    private readonly IVestingAccelerationEngine _accelerationEngine;
    private readonly ILogger<VestingAccelerationsController> _logger;

    public VestingAccelerationsController(
        IVestingAccelerationEngine accelerationEngine,
        ILogger<VestingAccelerationsController> logger)
    {
        _accelerationEngine = accelerationEngine;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as acelerações aplicadas a um grant
    /// </summary>
    [HttpGet("api/grants/{grantId:guid}/accelerations")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VestingAccelerationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGrant(Guid grantId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _accelerationEngine.GetByGrantAsync(clientId, grantId);
        return Ok(ApiResponse<IEnumerable<VestingAccelerationResponse>>.Ok(result));
    }

    /// <summary>
    /// Pré-visualiza o impacto de uma aceleração sem aplicá-la
    /// </summary>
    [HttpGet("api/grant-milestones/{milestoneId:guid}/acceleration-preview")]
    [ProducesResponseType(typeof(ApiResponse<AccelerationPreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Preview(Guid milestoneId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _accelerationEngine.GetPreviewAsync(milestoneId, clientId);
        return Ok(ApiResponse<AccelerationPreviewResponse>.Ok(result));
    }

    /// <summary>
    /// Aplica aceleração de vesting pelo milestone atingido e verificado
    /// </summary>
    [HttpPost("api/grant-milestones/{milestoneId:guid}/apply-acceleration")]
    [ProducesResponseType(typeof(ApiResponse<VestingAccelerationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyAcceleration(Guid milestoneId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _accelerationEngine.ApplyAccelerationAsync(milestoneId, clientId, userId);
        _logger.LogInformation("Aceleração aplicada para milestone {MilestoneId} — novo término: {NewEndDate}",
            milestoneId, result.NewVestingEndDate);
        return Ok(ApiResponse<VestingAccelerationResponse>.Ok(result, "Aceleração aplicada com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
