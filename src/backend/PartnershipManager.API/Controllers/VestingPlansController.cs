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
/// Gerenciamento de planos de vesting
/// </summary>
[ApiController]
[Route("api/vesting-plans")]
[Authorize]
[Produces("application/json")]
public class VestingPlansController : ControllerBase
{
    private readonly IVestingPlanService _vestingPlanService;
    private readonly ILogger<VestingPlansController> _logger;

    public VestingPlansController(IVestingPlanService vestingPlanService, ILogger<VestingPlansController> logger)
    {
        _vestingPlanService = vestingPlanService;
        _logger = logger;
    }

    /// <summary>
    /// Lista planos de vesting com paginação e filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _vestingPlanService.GetPagedAsync(clientId, companyId, page, pageSize, search, status);
        return Ok(ApiResponse<VestingPlanListResponse>.Ok(result));
    }

    /// <summary>
    /// Lista todos os planos ativos de uma empresa (sem paginação)
    /// </summary>
    [HttpGet("by-company/{companyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VestingPlanResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCompany(Guid companyId, [FromQuery] string? status = null)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _vestingPlanService.GetByCompanyAsync(clientId, companyId, status);
        return Ok(ApiResponse<IEnumerable<VestingPlanResponse>>.Ok(result));
    }

    /// <summary>
    /// Obtém plano de vesting por id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _vestingPlanService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<VestingPlanResponse>.Ok(result));
    }

    /// <summary>
    /// Cria novo plano de vesting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVestingPlanRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _vestingPlanService.CreateAsync(clientId, request, userId);
        _logger.LogInformation("Plano de vesting criado: {VestingPlanId}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<VestingPlanResponse>.Ok(result, "Plano de vesting criado com sucesso"));
    }

    /// <summary>
    /// Atualiza plano de vesting (apenas rascunhos)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVestingPlanRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _vestingPlanService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Plano de vesting atualizado: {VestingPlanId}", id);
        return Ok(ApiResponse<VestingPlanResponse>.Ok(result, "Plano atualizado com sucesso"));
    }

    /// <summary>
    /// Ativa um plano de vesting (Draft → Active)
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _vestingPlanService.ActivateAsync(id, clientId, userId);
        _logger.LogInformation("Plano de vesting ativado: {VestingPlanId}", id);
        return Ok(ApiResponse<VestingPlanResponse>.Ok(result, "Plano ativado com sucesso"));
    }

    /// <summary>
    /// Desativa um plano de vesting (Active → Inactive)
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _vestingPlanService.DeactivateAsync(id, clientId, userId);
        return Ok(ApiResponse<VestingPlanResponse>.Ok(result, "Plano desativado"));
    }

    /// <summary>
    /// Arquiva um plano de vesting
    /// </summary>
    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(typeof(ApiResponse<VestingPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Archive(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _vestingPlanService.ArchiveAsync(id, clientId, userId);
        return Ok(ApiResponse<VestingPlanResponse>.Ok(result, "Plano arquivado"));
    }

    /// <summary>
    /// Remove (soft delete) um plano de vesting
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _vestingPlanService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Plano de vesting removido: {VestingPlanId}", id);
        return Ok(ApiResponse.Ok("Plano removido com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
