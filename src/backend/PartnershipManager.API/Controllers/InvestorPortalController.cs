using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Communication;
using PartnershipManager.Application.DTOs.Portal;
using PartnershipManager.Application.Services;

namespace PartnershipManager.API.Controllers;

[Route("api/portal")]
[Authorize]
[Produces("application/json")]
public class InvestorPortalController : BaseApiController
{
    private readonly IInvestorPortalService _service;
    private readonly ILogger<InvestorPortalController> _logger;

    public InvestorPortalController(IInvestorPortalService service, ILogger<InvestorPortalController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var result = await _service.GetSummaryAsync(userId, companyId);
            return Ok(ApiResponse<InvestorSummaryResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar sumário do investidor");
            return StatusCode(500, new { message = "Erro ao buscar sumário" });
        }
    }

    [HttpGet("communications")]
    public async Task<IActionResult> GetCommunications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var result = await _service.GetCommunicationsAsync(userId, companyId, page, pageSize);
            return Ok(ApiResponse<PagedResult<CommunicationListResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar comunicações do portal");
            return StatusCode(500, new { message = "Erro ao buscar comunicações" });
        }
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var userId = GetRequiredUserId();
            var companyId = HttpContext.GetRequiredCompanyId();
            var docs = await _service.GetDocumentsAsync(userId, companyId);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(docs.Select(d => new
            {
                d.Id, d.Name, d.DocumentType, d.FileName, d.FileSizeBytes,
                d.MimeType, d.Visibility, d.IsVerified, d.CreatedAt
            })));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar documentos do portal");
            return StatusCode(500, new { message = "Erro ao buscar documentos" });
        }
    }

}
