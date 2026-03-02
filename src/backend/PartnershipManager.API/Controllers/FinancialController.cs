using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Financial.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Infrastructure.Services.Financial;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/financial")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly IFinancialPeriodService _service;

    public FinancialController(IFinancialPeriodService service)
    {
        _service = service;
    }

    // ─── GET api/financial?companyId=&year=&status=&page=&pageSize= ──────────

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid companyId,
        [FromQuery] int? year = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetPagedAsync(clientId, companyId, page, pageSize, year, status);
        return Ok(ApiResponse<FinancialPeriodListResponse>.Ok(result));
    }

    // ─── GET api/financial/{id} ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── GET api/financial/dashboard?companyId=&year= ───────────────────────

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<FinancialDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid companyId, [FromQuery] int year)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var result = await _service.GetDashboardAsync(clientId, companyId, year);
        return Ok(ApiResponse<FinancialDashboardResponse>.Ok(result));
    }

    // ─── POST api/financial ──────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFinancialPeriodRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();
        var result = await _service.CreateAsync(clientId, request, userId);
        return Created($"api/financial/{result.Id}", ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── PUT api/financial/{id} ──────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFinancialPeriodRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpdateNotesAsync(id, clientId, request, userId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── POST api/financial/{id}/submit ─────────────────────────────────────

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.SubmitAsync(id, clientId, userId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── POST api/financial/{id}/approve ────────────────────────────────────

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.ApproveAsync(id, clientId, userId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── POST api/financial/{id}/lock ────────────────────────────────────────

    [HttpPost("{id:guid}/lock")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lock([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.LockAsync(id, clientId, userId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── POST api/financial/{id}/return-to-submitted ─────────────────────────

    [HttpPost("{id:guid}/return-to-submitted")]
    [ProducesResponseType(typeof(ApiResponse<FinancialPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReturnToSubmitted([FromRoute] Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.ReturnToSubmittedAsync(id, clientId, userId);
        return Ok(ApiResponse<FinancialPeriodResponse>.Ok(result));
    }

    // ─── PUT api/financial/{id}/metrics/revenue ──────────────────────────────

    [HttpPut("{id:guid}/metrics/revenue")]
    [ProducesResponseType(typeof(ApiResponse<FinancialMetricResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertRevenue([FromRoute] Guid id, [FromBody] UpsertRevenueRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpsertRevenueAsync(id, clientId, request, userId);
        return Ok(ApiResponse<FinancialMetricResponse>.Ok(result));
    }

    // ─── PUT api/financial/{id}/metrics/cash-burn ────────────────────────────

    [HttpPut("{id:guid}/metrics/cash-burn")]
    [ProducesResponseType(typeof(ApiResponse<FinancialMetricResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertCashBurn([FromRoute] Guid id, [FromBody] UpsertCashBurnRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpsertCashBurnAsync(id, clientId, request, userId);
        return Ok(ApiResponse<FinancialMetricResponse>.Ok(result));
    }

    // ─── PUT api/financial/{id}/metrics/unit-economics ───────────────────────

    [HttpPut("{id:guid}/metrics/unit-economics")]
    [ProducesResponseType(typeof(ApiResponse<FinancialMetricResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertUnitEconomics([FromRoute] Guid id, [FromBody] UpsertUnitEconomicsRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpsertUnitEconomicsAsync(id, clientId, request, userId);
        return Ok(ApiResponse<FinancialMetricResponse>.Ok(result));
    }

    // ─── PUT api/financial/{id}/metrics/profitability ────────────────────────

    [HttpPut("{id:guid}/metrics/profitability")]
    [ProducesResponseType(typeof(ApiResponse<FinancialMetricResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertProfitability([FromRoute] Guid id, [FromBody] UpsertProfitabilityRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId() ?? Guid.Empty;
        var result = await _service.UpsertProfitabilityAsync(id, clientId, request, userId);
        return Ok(ApiResponse<FinancialMetricResponse>.Ok(result));
    }

    // ─── DELETE api/financial/{id} ───────────────────────────────────────────

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
