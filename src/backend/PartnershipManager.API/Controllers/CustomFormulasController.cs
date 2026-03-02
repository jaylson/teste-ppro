using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.CustomFormulas.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services.CustomFormulas;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/custom-formulas")]
[Authorize]
public class CustomFormulasController : ControllerBase
{
    private readonly ICustomFormulaService _service;

    public CustomFormulasController(ICustomFormulaService service)
    {
        _service = service;
    }

    // ─── GET api/custom-formulas?companyId=&isActive=&sectorTag=&page=&pageSize= ─

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid companyId,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sectorTag = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetPagedAsync(clientId, companyId, page, pageSize, isActive, sectorTag);
        return Ok(ApiResponse<CustomFormulaListResponse>.Ok(result));
    }

    // ─── GET api/custom-formulas/{id} ────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<CustomFormulaResponse>.Ok(result));
    }

    // ─── GET api/custom-formulas/{id}/versions ───────────────────────────────

    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FormulaVersionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetVersionsAsync(id, clientId);
        return Ok(ApiResponse<IEnumerable<FormulaVersionResponse>>.Ok(result));
    }

    // ─── POST api/custom-formulas ────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCustomFormulaRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.CreateAsync(clientId, request, userId);
        return Created($"api/custom-formulas/{result.Id}", ApiResponse<CustomFormulaResponse>.Ok(result));
    }

    // ─── PUT api/custom-formulas/{id} ────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFormulaMetadataRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpdateMetadataAsync(id, clientId, request, userId);
        return Ok(ApiResponse<CustomFormulaResponse>.Ok(result));
    }

    // ─── POST api/custom-formulas/{id}/versions ──────────────────────────────

    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(typeof(ApiResponse<FormulaVersionResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> PublishVersion([FromRoute] Guid id, [FromBody] PublishNewFormulaVersionRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.PublishNewVersionAsync(id, clientId, request, userId);
        return Created($"api/custom-formulas/{id}/versions/{result.Id}", ApiResponse<FormulaVersionResponse>.Ok(result));
    }

    // ─── POST api/custom-formulas/test ──────────────────────────────────────

    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<TestFormulaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Test([FromBody] TestFormulaRequest request)
    {
        var result = await _service.TestFormulaAsync(request);
        return Ok(ApiResponse<TestFormulaResponse>.Ok(result));
    }

    // ─── POST api/custom-formulas/{id}/activate ──────────────────────────────

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.ActivateAsync(id, clientId, userId);
        return Ok(ApiResponse<CustomFormulaResponse>.Ok(result));
    }

    // ─── POST api/custom-formulas/{id}/deactivate ────────────────────────────

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<CustomFormulaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.DeactivateAsync(id, clientId, userId);
        return Ok(ApiResponse<CustomFormulaResponse>.Ok(result));
    }

    // ─── DELETE api/custom-formulas/{id} ─────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _service.DeleteAsync(id, clientId, userId);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
