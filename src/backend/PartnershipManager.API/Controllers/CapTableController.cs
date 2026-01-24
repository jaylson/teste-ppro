using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Shares.DTOs;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Visualização do Cap Table (quadro societário)
/// </summary>
[ApiController]
[Route("api/cap-table")]
[Authorize]
[Produces("application/json")]
public class CapTableController : ControllerBase
{
    private readonly IShareService _shareService;
    private readonly ILogger<CapTableController> _logger;

    public CapTableController(IShareService shareService, ILogger<CapTableController> logger)
    {
        _shareService = shareService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o Cap Table de uma empresa
    /// </summary>
    /// <param name="companyId">ID da empresa</param>
    /// <returns>Cap Table com lista de sócios, ações e percentuais</returns>
    [HttpGet("{companyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CapTableResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapTable(Guid companyId)
    {
        var clientId = HttpContext.GetRequiredClientId();

        _logger.LogInformation("Consultando Cap Table da empresa {CompanyId}", companyId);

        var capTable = await _shareService.GetCapTableAsync(clientId, companyId);

        return Ok(ApiResponse<CapTableResponse>.Ok(capTable));
    }

    /// <summary>
    /// Obtém o resumo do Cap Table por tipo de sócio
    /// </summary>
    [HttpGet("{companyId:guid}/summary-by-type")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CapTableSummaryByType>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummaryByType(Guid companyId)
    {
        var clientId = HttpContext.GetRequiredClientId();

        var capTable = await _shareService.GetCapTableAsync(clientId, companyId);

        return Ok(ApiResponse<IEnumerable<CapTableSummaryByType>>.Ok(capTable.SummaryByType));
    }

    /// <summary>
    /// Obtém o resumo do Cap Table por classe de ações
    /// </summary>
    [HttpGet("{companyId:guid}/summary-by-class")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CapTableSummaryByClass>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummaryByClass(Guid companyId)
    {
        var clientId = HttpContext.GetRequiredClientId();

        var capTable = await _shareService.GetCapTableAsync(clientId, companyId);

        return Ok(ApiResponse<IEnumerable<CapTableSummaryByClass>>.Ok(capTable.SummaryByClass));
    }
}
