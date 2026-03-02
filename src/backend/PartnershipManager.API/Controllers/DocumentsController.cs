using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Documents.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services.Documents;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service)
    {
        _service = service;
    }

    // ─── GET api/documents?companyId=&documentType=&visibility=&search=&page=&pageSize= ─

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DocumentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid companyId,
        [FromQuery] string? documentType = null,
        [FromQuery] string? visibility = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetPagedAsync(clientId, companyId, page, pageSize, documentType, visibility, search);
        return Ok(ApiResponse<DocumentListResponse>.Ok(result));
    }

    // ─── GET api/documents/by-entity?companyId=&entityType=&entityId= ────────

    [HttpGet("by-entity")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        [FromQuery] Guid companyId,
        [FromQuery] string entityType,
        [FromQuery] Guid entityId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetByEntityAsync(clientId, companyId, entityType, entityId);
        return Ok(ApiResponse<IEnumerable<DocumentResponse>>.Ok(result));
    }

    // ─── GET api/documents/{id} ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<DocumentResponse>.Ok(result));
    }

    // ─── POST api/documents ──────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _service.CreateAsync(clientId, request, userId);
        return Created($"api/documents/{result.Id}", ApiResponse<DocumentResponse>.Ok(result));
    }

    // ─── PUT api/documents/{id} ──────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDocumentMetadataRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpdateMetadataAsync(id, clientId, request, userId);
        return Ok(ApiResponse<DocumentResponse>.Ok(result));
    }

    // ─── POST api/documents/{id}/verify ─────────────────────────────────────

    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Verify([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.VerifyAsync(id, clientId, userId);
        return Ok(ApiResponse<DocumentResponse>.Ok(result));
    }

    // ─── DELETE api/documents/{id} ───────────────────────────────────────────

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
