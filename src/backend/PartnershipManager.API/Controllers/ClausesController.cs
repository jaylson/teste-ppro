using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Gestão da biblioteca de cláusulas
/// </summary>
[ApiController]
[Route("api/clauses")]
[Authorize]
[Produces("application/json")]
public class ClausesController : ControllerBase
{
    private readonly IClauseService _clauseService;
    private readonly ILogger<ClausesController> _logger;

    public ClausesController(
        IClauseService clauseService, 
        ILogger<ClausesController> logger)
    {
        _clauseService = clauseService;
        _logger = logger;
    }

    /// <summary>
    /// Lista cláusulas com filtros e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ClauseListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? clauseType = null,
        [FromQuery] bool? isMandatory = null,
        [FromQuery] bool? isActive = null)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();

        Domain.Enums.ClauseType? parsedClauseType = null;
        if (!string.IsNullOrWhiteSpace(clauseType) && Enum.TryParse<Domain.Enums.ClauseType>(clauseType, true, out var cType))
            parsedClauseType = cType;

        var result = await _clauseService.GetPagedAsync(
            clientId, page, pageSize, search, parsedClauseType, isMandatory, isActive);

        return Ok(ApiResponse<ClauseListResponse>.Ok(result));
    }

    /// <summary>
    /// Obtém cláusula por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClauseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var clause = await _clauseService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<ClauseResponse>.Ok(clause));
    }

    /// <summary>
    /// Lista cláusulas obrigatórias (devem estar em todos os contratos)
    /// </summary>
    [HttpGet("mandatory")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClauseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMandatoryClauses()
    {
        var clientId = HttpContext.GetRequiredClientId();
        var clauses = await _clauseService.GetMandatoryClausesAsync(clientId);
        return Ok(ApiResponse<IEnumerable<ClauseResponse>>.Ok(clauses));
    }

    /// <summary>
    /// Lista cláusulas ativas (para seleção em dropdowns)
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClauseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveClauses()
    {
        var clientId = HttpContext.GetRequiredClientId();
        var clauses = await _clauseService.GetActiveClausesAsync(clientId);
        return Ok(ApiResponse<IEnumerable<ClauseResponse>>.Ok(clauses));
    }

    /// <summary>
    /// Lista cláusulas por tipo/categoria (Governance, Financial, Operational, etc.)
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClauseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByType(string type)
    {
        if (!Enum.TryParse<Domain.Enums.ClauseType>(type, true, out var clauseType))
            return BadRequest(ApiResponse.Error($"Tipo de cláusula inválido: {type}"));

        var clientId = HttpContext.GetRequiredClientId();
        var clauses = await _clauseService.GetByTypeAsync(clientId, clauseType);
        return Ok(ApiResponse<IEnumerable<ClauseResponse>>.Ok(clauses));
    }

    /// <summary>
    /// Cria nova cláusula
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClauseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClauseRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var clause = await _clauseService.CreateAsync(clientId, request, userId);
        _logger.LogInformation("Cláusula criada: {ClauseId} - {ClauseName}", clause.Id, clause.Name);

        return CreatedAtAction(nameof(GetById), new { id = clause.Id }, 
            ApiResponse<ClauseResponse>.Ok(clause, "Cláusula criada com sucesso"));
    }

    /// <summary>
    /// Atualiza uma cláusula
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClauseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClauseRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var clause = await _clauseService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Cláusula atualizada: {ClauseId}", id);

        return Ok(ApiResponse<ClauseResponse>.Ok(clause, "Cláusula atualizada com sucesso"));
    }

    /// <summary>
    /// Remove (soft delete) uma cláusula
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        await _clauseService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Cláusula removida: {ClauseId}", id);

        return Ok(ApiResponse.Ok("Cláusula removida com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
