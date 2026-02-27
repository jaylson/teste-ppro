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
/// Gerenciamento de templates de milestones reutilizáveis por empresa
/// </summary>
[ApiController]
[Route("api/milestone-templates")]
[Authorize]
[Produces("application/json")]
public class MilestoneTemplatesController : ControllerBase
{
    private readonly IMilestoneTemplateService _templateService;
    private readonly ILogger<MilestoneTemplatesController> _logger;

    public MilestoneTemplatesController(
        IMilestoneTemplateService templateService,
        ILogger<MilestoneTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Lista templates de milestone com paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<MilestoneTemplateListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _templateService.GetPagedAsync(clientId, companyId, page, pageSize, category, isActive);
        return Ok(ApiResponse<MilestoneTemplateListResponse>.Ok(result));
    }

    /// <summary>
    /// Lista todos os templates ativos de uma empresa (sem paginação)
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MilestoneTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByCompany([FromQuery] Guid companyId, [FromQuery] bool activeOnly = true)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _templateService.GetByCompanyAsync(clientId, companyId, activeOnly);
        return Ok(ApiResponse<IEnumerable<MilestoneTemplateResponse>>.Ok(result));
    }

    /// <summary>
    /// Obtém template por id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MilestoneTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _templateService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<MilestoneTemplateResponse>.Ok(result));
    }

    /// <summary>
    /// Cria novo template de milestone
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MilestoneTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromQuery] Guid companyId, [FromBody] CreateMilestoneTemplateRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        // companyId vem como query param; garante que o DTO também o tenha para o validator
        var mergedRequest = request with { CompanyId = companyId };
        var result = await _templateService.CreateAsync(clientId, mergedRequest, userId);
        _logger.LogInformation("Template de milestone criado: {TemplateId}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<MilestoneTemplateResponse>.Ok(result, "Template criado com sucesso"));
    }

    /// <summary>
    /// Atualiza template de milestone
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MilestoneTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMilestoneTemplateRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _templateService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Template de milestone atualizado: {TemplateId}", id);
        return Ok(ApiResponse<MilestoneTemplateResponse>.Ok(result, "Template atualizado com sucesso"));
    }

    /// <summary>
    /// Ativa um template de milestone
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _templateService.ActivateAsync(id, clientId, userId);
        return Ok(ApiResponse.Ok("Template ativado com sucesso"));
    }

    /// <summary>
    /// Desativa um template de milestone
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _templateService.DeactivateAsync(id, clientId, userId);
        return Ok(ApiResponse.Ok("Template desativado com sucesso"));
    }

    /// <summary>
    /// Remove (soft delete) um template de milestone
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _templateService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Template de milestone removido: {TemplateId}", id);
        return Ok(ApiResponse.Ok("Template removido com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
