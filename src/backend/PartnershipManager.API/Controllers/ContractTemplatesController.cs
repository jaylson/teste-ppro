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
/// Gestão de templates de contratos
/// </summary>
[ApiController]
[Route("api/contract-templates")]
[Authorize]
[Produces("application/json")]
public class ContractTemplatesController : ControllerBase
{
    private readonly IContractTemplateService _templateService;
    private readonly ILogger<ContractTemplatesController> _logger;

    public ContractTemplatesController(
        IContractTemplateService templateService, 
        ILogger<ContractTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Lista templates de contratos com filtros e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ContractTemplateListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();

        Domain.Enums.ContractTemplateType? templateType = null;
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<Domain.Enums.ContractTemplateType>(type, true, out var parsedType))
            templateType = parsedType;

        var result = await _templateService.GetPagedAsync(clientId, page, pageSize, search, templateType, isActive);

        return Ok(ApiResponse<ContractTemplateListResponse>.Ok(result));
    }

    /// <summary>
    /// Obtém template por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var template = await _templateService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<ContractTemplateResponse>.Ok(template));
    }

    /// <summary>
    /// Lista templates ativos (para seleção em dropdowns)
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTemplates()
    {
        var clientId = HttpContext.GetRequiredClientId();
        var templates = await _templateService.GetActiveTemplatesAsync(clientId);
        return Ok(ApiResponse<IEnumerable<ContractTemplateResponse>>.Ok(templates));
    }

    /// <summary>
    /// Lista templates por tipo (Partnership, Employment, Service, NDA, etc.)
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractTemplateResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByType(string type)
    {
        if (!Enum.TryParse<Domain.Enums.ContractTemplateType>(type, true, out var templateType))
            return BadRequest(ApiResponse.Error($"Tipo de template inválido: {type}"));

        var clientId = HttpContext.GetRequiredClientId();
        var templates = await _templateService.GetByTypeAsync(clientId, templateType);
        return Ok(ApiResponse<IEnumerable<ContractTemplateResponse>>.Ok(templates));
    }

    /// <summary>
    /// Cria novo template de contrato
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContractTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContractTemplateRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var template = await _templateService.CreateAsync(clientId, request, userId);
        _logger.LogInformation("Template de contrato criado: {TemplateId} - {TemplateName}", template.Id, template.Name);

        return CreatedAtAction(nameof(GetById), new { id = template.Id }, 
            ApiResponse<ContractTemplateResponse>.Ok(template, "Template criado com sucesso"));
    }

    /// <summary>
    /// Atualiza um template de contrato
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractTemplateRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var template = await _templateService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Template de contrato atualizado: {TemplateId}", id);

        return Ok(ApiResponse<ContractTemplateResponse>.Ok(template, "Template atualizado com sucesso"));
    }

    /// <summary>
    /// Clona um template de contrato com novo código e nome
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(ApiResponse<ContractTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Clone(Guid id, [FromBody] CloneContractTemplateRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var clonedTemplate = await _templateService.CloneAsync(id, clientId, request, userId);
        _logger.LogInformation("Template clonado: {OriginalId} -> {ClonedId}", id, clonedTemplate.Id);

        return CreatedAtAction(nameof(GetById), new { id = clonedTemplate.Id }, 
            ApiResponse<ContractTemplateResponse>.Ok(clonedTemplate, "Template clonado com sucesso"));
    }

    /// <summary>
    /// Remove (soft delete) um template de contrato
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        await _templateService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Template de contrato removido: {TemplateId}", id);

        return Ok(ApiResponse.Ok("Template removido com sucesso"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
