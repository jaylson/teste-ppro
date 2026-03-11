using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Communication;
using PartnershipManager.Application.Services;

namespace PartnershipManager.API.Controllers;

[Route("api/communications")]
[Authorize]
[Produces("application/json")]
public class CommunicationsController : BaseApiController
{
    private readonly ICommunicationService _service;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(ICommunicationService service, ILogger<CommunicationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? commType = null,
        [FromQuery] bool? isPublished = null)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId();
            var result = await _service.GetByCompanyAsync(companyId, userId, page, pageSize, search, commType, isPublished);
            return Ok(ApiResponse<PagedResult<CommunicationListResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar comunicações");
            return StatusCode(500, new { message = "Erro ao buscar comunicações" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId();
            var result = await _service.GetByIdAsync(id, companyId, userId);
            if (result == null) return NotFound(new { message = "Comunicação não encontrada" });
            return Ok(ApiResponse<CommunicationResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar comunicação {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar comunicação" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommunicationRequest request)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            var id = await _service.CreateAsync(companyId, request, userId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar comunicação");
            return StatusCode(500, new { message = "Erro ao criar comunicação" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommunicationRequest request)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            await _service.UpdateAsync(id, companyId, request, userId);
            return Ok(ApiResponse.Ok("Comunicação atualizada com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar comunicação {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar comunicação" });
        }
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            await _service.PublishAsync(id, companyId, userId);
            return Ok(ApiResponse.Ok("Comunicação publicada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar comunicação {Id}", id);
            return StatusCode(500, new { message = "Erro ao publicar comunicação" });
        }
    }

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> TrackView(Guid id, [FromBody] TrackViewRequest? request)
    {
        try
        {
            var userId = GetUserId();
            if (userId.HasValue)
                await _service.TrackViewAsync(id, userId.Value, request?.ViewDurationSecs);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar visualização {Id}", id);
            return StatusCode(500, new { message = "Erro ao registrar visualização" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            await _service.DeleteAsync(id, companyId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar comunicação {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar comunicação" });
        }
    }
}
