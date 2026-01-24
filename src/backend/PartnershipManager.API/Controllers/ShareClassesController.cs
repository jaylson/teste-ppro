using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.ShareClasses.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// CRUD de classes de ações
/// </summary>
[ApiController]
[Route("api/share-classes")]
[Authorize]
[Produces("application/json")]
public class ShareClassesController : ControllerBase
{
    private readonly IShareClassService _shareClassService;
    private readonly ILogger<ShareClassesController> _logger;

    public ShareClassesController(IShareClassService shareClassService, ILogger<ShareClassesController> logger)
    {
        _shareClassService = shareClassService;
        _logger = logger;
    }

    /// <summary>
    /// Lista classes de ações com filtros e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ShareClassListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);

        var clientId = HttpContext.GetRequiredClientId();

        var result = await _shareClassService.GetPagedAsync(clientId, companyId, page, pageSize, search, status);

        return Ok(ApiResponse<ShareClassListResponse>.Ok(result));
    }

    /// <summary>
    /// Lista classes de ações de uma empresa específica (resumido)
    /// </summary>
    [HttpGet("by-company/{companyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShareClassSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCompany(Guid companyId)
    {
        var clientId = HttpContext.GetRequiredClientId();

        var result = await _shareClassService.GetByCompanyAsync(clientId, companyId);

        return Ok(ApiResponse<IEnumerable<ShareClassSummaryResponse>>.Ok(result));
    }

    /// <summary>
    /// Obtém classe de ações por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShareClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var shareClass = await _shareClassService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<ShareClassResponse>.Ok(shareClass));
    }

    /// <summary>
    /// Cria nova classe de ações
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShareClassResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateShareClassRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var shareClass = await _shareClassService.CreateAsync(clientId, request, userId);
        _logger.LogInformation("Classe de ações criada: {ShareClassId} - {Code}", shareClass.Id, shareClass.Code);

        return CreatedAtAction(nameof(GetById), new { id = shareClass.Id }, 
            ApiResponse<ShareClassResponse>.Ok(shareClass, "Classe de ações criada com sucesso"));
    }

    /// <summary>
    /// Atualiza uma classe de ações
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShareClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShareClassRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var shareClass = await _shareClassService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Classe de ações atualizada: {ShareClassId}", id);

        return Ok(ApiResponse<ShareClassResponse>.Ok(shareClass, "Classe de ações atualizada com sucesso"));
    }

    /// <summary>
    /// Remove (soft delete) uma classe de ações
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        await _shareClassService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Classe de ações removida: {ShareClassId}", id);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("userId")?.Value 
                       ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
