using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.DataRoom;
using PartnershipManager.Application.Services;

namespace PartnershipManager.API.Controllers;

[ApiController]
[Route("api/dataroom")]
[Authorize]
[Produces("application/json")]
public class DataRoomController : ControllerBase
{
    private readonly IDataRoomService _service;
    private readonly ILogger<DataRoomController> _logger;

    public DataRoomController(IDataRoomService service, ILogger<DataRoomController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            var result = await _service.GetOrCreateDataRoomAsync(companyId, userId);
            return Ok(ApiResponse<DataRoomResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter data room");
            return StatusCode(500, new { message = "Erro ao obter data room" });
        }
    }

    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders([FromQuery] Guid? parentId = null)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var result = await _service.GetFoldersAsync(companyId, parentId);
            return Ok(ApiResponse<IEnumerable<DataRoomFolderResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pastas");
            return StatusCode(500, new { message = "Erro ao buscar pastas" });
        }
    }

    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
    {
        try
        {
            var companyId = HttpContext.GetRequiredCompanyId();
            var userId = GetUserId() ?? Guid.Empty;
            var id = await _service.CreateFolderAsync(companyId, request, userId);
            return CreatedAtAction(nameof(GetFolders), new { }, new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pasta");
            return StatusCode(500, new { message = "Erro ao criar pasta" });
        }
    }

    [HttpDelete("folders/{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        try
        {
            await _service.DeleteFolderAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar pasta {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar pasta" });
        }
    }

    [HttpGet("folders/{id:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid id)
    {
        try
        {
            var result = await _service.GetDocumentsInFolderAsync(id);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Select(d => new
            {
                d.Id, d.Name, d.DocumentType, d.FileName, d.FileSizeBytes, d.MimeType,
                d.Visibility, d.IsVerified, d.CreatedAt
            })));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar documentos da pasta {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar documentos" });
        }
    }

    [HttpPost("folders/{id:guid}/documents")]
    public async Task<IActionResult> AddDocument(Guid id, [FromBody] AddDocumentToFolderRequest request)
    {
        try
        {
            var userId = GetUserId() ?? Guid.Empty;
            await _service.AddDocumentToFolderAsync(id, request.DocumentId, userId);
            return Ok(ApiResponse.Ok("Documento adicionado à pasta"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar documento à pasta {Id}", id);
            return StatusCode(500, new { message = "Erro ao adicionar documento" });
        }
    }

    [HttpDelete("folders/{folderId:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> RemoveDocument(Guid folderId, Guid documentId)
    {
        try
        {
            await _service.RemoveDocumentFromFolderAsync(folderId, documentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover documento da pasta {FolderId}", folderId);
            return StatusCode(500, new { message = "Erro ao remover documento" });
        }
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
