using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Valuation.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services.Valuation;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/valuations")]
[Authorize]
public class ValuationController : ControllerBase
{
    private readonly IValuationService _valuationService;

    public ValuationController(IValuationService valuationService)
    {
        _valuationService = valuationService;
    }

    // ─── GET api/valuations?companyId=&status=&page=&pageSize= ───────────────

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ValuationListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid? companyId,
        [FromQuery] string? status = null,
        [FromQuery] string? eventType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _valuationService.GetPagedAsync(clientId, companyId, page, pageSize, status, eventType);
        return Ok(ApiResponse<ValuationListResponse>.Ok(result));
    }

    // ─── GET api/valuations/{id} ─────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _valuationService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations ─────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateValuationRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _valuationService.CreateAsync(clientId, request, userId);
        return Created($"api/valuations/{result.Id}", ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── PUT api/valuations/{id} ─────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateValuationRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.UpdateAsync(id, clientId, request, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/submit ─────────────────────────────────────

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.SubmitAsync(id, clientId, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/approve ────────────────────────────────────

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.ApproveAsync(id, clientId, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/reject ─────────────────────────────────────

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject([FromRoute] Guid id, [FromBody] RejectValuationRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.RejectAsync(id, clientId, request.Reason, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/return-to-draft ────────────────────────────

    [HttpPost("{id:guid}/return-to-draft")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReturnToDraft([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.ReturnToDraftAsync(id, clientId, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/methods ────────────────────────────────────

    [HttpPost("{id:guid}/methods")]
    [ProducesResponseType(typeof(ApiResponse<ValuationMethodResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMethod([FromRoute] Guid id, [FromBody] AddValuationMethodRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.AddMethodAsync(id, clientId, request, userId);
        return Created($"api/valuations/{id}/methods/{result.Id}", ApiResponse<ValuationMethodResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/methods/{methodId}/calculate ──────────────

    [HttpPost("{id:guid}/methods/{methodId:guid}/calculate")]
    [ProducesResponseType(typeof(ApiResponse<CalculateMethodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateMethod(
        [FromRoute] Guid id,
        [FromRoute] Guid methodId,
        [FromBody] CalculateMethodRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.CalculateMethodAsync(id, methodId, clientId, request, userId);
        return Ok(ApiResponse<CalculateMethodResponse>.Ok(result));
    }

    // ─── POST api/valuations/{id}/select-method ──────────────────────────────

    [HttpPost("{id:guid}/select-method")]
    [ProducesResponseType(typeof(ApiResponse<ValuationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SelectMethod([FromRoute] Guid id, [FromBody] SelectMethodRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _valuationService.SelectMethodAsync(id, clientId, request.MethodId, userId);
        return Ok(ApiResponse<ValuationResponse>.Ok(result));
    }

    // ─── DELETE api/valuations/{id} ──────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        await _valuationService.DeleteAsync(id, clientId, userId);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
