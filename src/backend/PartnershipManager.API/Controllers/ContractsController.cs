using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Gestão completa de contratos com partes, cláusulas e documentos
/// </summary>
[ApiController]
[Route("api/contracts")]
[Authorize]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IContractService contractService, 
        ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    #region CRUD Básico de Contratos

    /// <summary>
    /// Lista contratos com filtros e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ContractListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? contractType = null)
    {
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        var clientId = HttpContext.GetRequiredClientId();

        Domain.Enums.ContractStatus? contractStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.ContractStatus>(status, true, out var parsedStatus))
            contractStatus = parsedStatus;

        Domain.Enums.ContractTemplateType? templateType = null;
        if (!string.IsNullOrWhiteSpace(contractType) && Enum.TryParse<Domain.Enums.ContractTemplateType>(contractType, true, out var parsedType))
            templateType = parsedType;

        var result = await _contractService.GetPagedAsync(
            clientId, companyId, page, pageSize, search, contractStatus, templateType);

        return Ok(ApiResponse<ContractListResponse>.Ok(result));
    }

    /// <summary>
    /// Obtém contrato por ID (sem detalhes de parties e clauses)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var contract = await _contractService.GetByIdAsync(id, clientId);
        return Ok(ApiResponse<ContractResponse>.Ok(contract));
    }

    /// <summary>
    /// Obtém contrato com detalhes completos (parties, clauses, variáveis)
    /// </summary>
    [HttpGet("{id:guid}/details")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithDetails(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var contract = await _contractService.GetWithDetailsAsync(id, clientId);
        return Ok(ApiResponse<ContractResponse>.Ok(contract));
    }

    /// <summary>
    /// Lista contratos de uma empresa específica
    /// </summary>
    [HttpGet("by-company/{companyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCompany(Guid companyId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var contracts = await _contractService.GetByCompanyAsync(clientId, companyId);
        return Ok(ApiResponse<IEnumerable<ContractResponse>>.Ok(contracts));
    }

    /// <summary>
    /// Lista contratos por status (Draft, PendingReview, Approved, etc.)
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        if (!Enum.TryParse<Domain.Enums.ContractStatus>(status, true, out var contractStatus))
            return BadRequest(ApiResponse.Error($"Status de contrato inválido: {status}"));

        var clientId = HttpContext.GetRequiredClientId();
        var contracts = await _contractService.GetByStatusAsync(clientId, contractStatus);
        return Ok(ApiResponse<IEnumerable<ContractResponse>>.Ok(contracts));
    }

    /// <summary>
    /// Lista contratos expirados (expiration_date &lt; NOW())
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiredContracts()
    {
        var clientId = HttpContext.GetRequiredClientId();
        var contracts = await _contractService.GetExpiredContractsAsync(clientId);
        return Ok(ApiResponse<IEnumerable<ContractResponse>>.Ok(contracts));
    }

    /// <summary>
    /// Cria novo contrato
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.CreateAsync(clientId, request, userId);
        _logger.LogInformation("Contrato criado: {ContractId} - {ContractTitle}", contract.Id, contract.Title);

        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, 
            ApiResponse<ContractResponse>.Ok(contract, "Contrato criado com sucesso"));
    }

    /// <summary>
    /// Atualiza um contrato (apenas contratos em Draft ou PendingReview)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.UpdateAsync(id, clientId, request, userId);
        _logger.LogInformation("Contrato atualizado: {ContractId}", id);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Contrato atualizado com sucesso"));
    }

    /// <summary>
    /// Remove (soft delete) um contrato (não permite remover contratos Executed)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        await _contractService.DeleteAsync(id, clientId, userId);
        _logger.LogInformation("Contrato removido: {ContractId}", id);

        return Ok(ApiResponse.Ok("Contrato removido com sucesso"));
    }

    #endregion

    #region Gestão de Status do Contrato

    /// <summary>
    /// Atualiza status do contrato (submeter para revisão, aprovar, enviar para assinatura, marcar como executado, cancelar)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateContractStatusRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.UpdateStatusAsync(id, clientId, request, userId);
        _logger.LogInformation("Status do contrato atualizado: {ContractId} -> {Status}", id, request.Status);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, $"Status atualizado para '{request.Status}'"));
    }

    #endregion

    #region Gestão de Partes do Contrato

    /// <summary>
    /// Adiciona uma parte ao contrato (signatário, destinatário ou testemunha)
    /// </summary>
    [HttpPost("{id:guid}/parties")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddParty(Guid id, [FromBody] AddContractPartyRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.AddPartyAsync(id, clientId, request, userId);
        _logger.LogInformation("Parte adicionada ao contrato: {ContractId} - {PartyName} ({PartyType})", 
            id, request.PartyName, request.PartyType);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Parte adicionada ao contrato"));
    }

    /// <summary>
    /// Remove uma parte do contrato (apenas se não assinado)
    /// </summary>
    [HttpDelete("{id:guid}/parties/{partyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveParty(Guid id, Guid partyId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.RemovePartyAsync(id, clientId, partyId, userId);
        _logger.LogInformation("Parte removida do contrato: {ContractId} - PartyId {PartyId}", 
            id, partyId);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Parte removida do contrato"));
    }

    /// <summary>
    /// Atualiza informações de uma parte do contrato
    /// </summary>
    [HttpPut("{id:guid}/parties/{partyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateParty(Guid id, Guid partyId, [FromBody] UpdateContractPartyRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.UpdatePartyAsync(id, clientId, partyId, request, userId);
        _logger.LogInformation("Parte atualizada no contrato: {ContractId} - PartyId {PartyId}", 
            id, partyId);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Parte atualizada com sucesso"));
    }

    #endregion

    #region Gestão de Cláusulas do Contrato

    /// <summary>
    /// Adiciona uma cláusula ao contrato com conteúdo customizado e variáveis
    /// </summary>
    [HttpPost("{id:guid}/clauses")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddClause(Guid id, [FromBody] AddContractClauseRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.AddClauseAsync(id, clientId, request, userId);
        _logger.LogInformation("Cláusula adicionada ao contrato: {ContractId} - ClauseId {ClauseId}", 
            id, request.ClauseId);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Cláusula adicionada ao contrato"));
    }

    /// <summary>
    /// Remove uma cláusula do contrato (apenas se não obrigatória e contrato não executado)
    /// </summary>
    [HttpDelete("{id:guid}/clauses/{clauseId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveClause(Guid id, Guid clauseId)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.RemoveClauseAsync(id, clientId, clauseId, userId);
        _logger.LogInformation("Cláusula removida do contrato: {ContractId} - ClauseId {ClauseId}", 
            id, clauseId);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Cláusula removida do contrato"));
    }

    /// <summary>
    /// Atualiza a ordem das cláusulas no contrato
    /// </summary>
    [HttpPut("{id:guid}/clauses/order")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderClauses(Guid id, [FromBody] ReorderClausesRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.ReorderClausesAsync(id, clientId, request, userId);
        _logger.LogInformation("Cláusulas reordenadas no contrato: {ContractId}", id);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Cláusulas reordenadas com sucesso"));
    }

    /// <summary>
    /// Atualiza conteúdo customizado de uma cláusula
    /// </summary>
    [HttpPut("{id:guid}/clauses/{clauseId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateClause(Guid id, Guid clauseId, [FromBody] UpdateContractClauseRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.UpdateClauseAsync(id, clientId, clauseId, request, userId);
        _logger.LogInformation("Cláusula atualizada no contrato: {ContractId} - ClauseId {ClauseId}", 
            id, clauseId);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Cláusula atualizada com sucesso"));
    }

    #endregion

    #region Gestão de Documentos

    /// <summary>
    /// Anexa documento gerado ou assinado ao contrato (PDF, hash para integridade)
    /// </summary>
    [HttpPost("{id:guid}/documents")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AttachDocument(Guid id, [FromBody] AttachDocumentRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();
        var userId = GetUserId();

        var contract = await _contractService.AttachDocumentAsync(id, clientId, request, userId);
        _logger.LogInformation("Documento anexado ao contrato: {ContractId} - {DocumentPath}", 
            id, request.DocumentPath);

        return Ok(ApiResponse<ContractResponse>.Ok(contract, "Documento anexado com sucesso"));
    }

    #endregion

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
