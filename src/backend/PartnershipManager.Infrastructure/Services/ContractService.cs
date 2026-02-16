using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Interface for Contract service operations
/// </summary>
public interface IContractService
{
    Task<ContractListResponse> GetPagedAsync(
        Guid clientId,
        Guid? companyId,
        int page,
        int pageSize,
        string? search = null,
        ContractStatus? status = null,
        ContractTemplateType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    Task<ContractResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<ContractResponse> GetWithDetailsAsync(Guid id, Guid clientId);
    Task<IEnumerable<ContractResponse>> GetByCompanyAsync(Guid companyId, Guid clientId);
    Task<IEnumerable<ContractResponse>> GetByStatusAsync(Guid clientId, ContractStatus status);
    Task<IEnumerable<ContractResponse>> GetExpiredContractsAsync(Guid clientId);
    Task<ContractResponse> CreateAsync(Guid clientId, CreateContractRequest request, Guid? userId = null);
    Task<ContractResponse> UpdateAsync(Guid id, Guid clientId, UpdateContractRequest request, Guid? userId = null);
    Task<ContractResponse> UpdateStatusAsync(Guid id, Guid clientId, UpdateContractStatusRequest request, Guid? userId = null);
    Task<ContractResponse> AttachDocumentAsync(Guid id, Guid clientId, AttachDocumentRequest request, Guid? userId = null);
    Task<ContractResponse> AddPartyAsync(Guid id, Guid clientId, AddContractPartyRequest request, Guid? userId = null);
    Task<ContractResponse> RemovePartyAsync(Guid id, Guid clientId, Guid partyId, Guid? userId = null);
    Task<ContractResponse> UpdatePartyAsync(Guid id, Guid clientId, Guid partyId, UpdateContractPartyRequest request, Guid? userId = null);
    Task<ContractResponse> AddClauseAsync(Guid id, Guid clientId, AddContractClauseRequest request, Guid? userId = null);
    Task<ContractResponse> RemoveClauseAsync(Guid id, Guid clientId, Guid clauseId, Guid? userId = null);
    Task<ContractResponse> UpdateClauseAsync(Guid id, Guid clientId, Guid clauseId, UpdateContractClauseRequest request, Guid? userId = null);
    Task<ContractResponse> ReorderClausesAsync(Guid id, Guid clientId, ReorderClausesRequest request, Guid? userId = null);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

/// <summary>
/// Service for managing contracts
/// </summary>
public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractTemplateRepository _templateRepository;
    private readonly IClauseRepository _clauseRepository;
    private readonly ICompanyRepository _companyRepository;

    public ContractService(
        IContractRepository contractRepository,
        IContractTemplateRepository templateRepository,
        IClauseRepository clauseRepository,
        ICompanyRepository companyRepository)
    {
        _contractRepository = contractRepository;
        _templateRepository = templateRepository;
        _clauseRepository = clauseRepository;
        _companyRepository = companyRepository;
    }

    public async Task<ContractListResponse> GetPagedAsync(
        Guid clientId,
        Guid? companyId,
        int page,
        int pageSize,
        string? search = null,
        ContractStatus? status = null,
        ContractTemplateType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var (items, total) = await _contractRepository.GetPagedAsync(
            clientId, page, pageSize, companyId, search, 
            status?.ToString(), type?.ToString(), fromDate, toDate);
        
        var responseItems = items.Select(MapToResponse);
        return new ContractListResponse(responseItems, total, page, pageSize);
    }

    public async Task<ContractResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var contract = await _contractRepository.GetByIdAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }
        return MapToResponse(contract);
    }

    public async Task<ContractResponse> GetWithDetailsAsync(Guid id, Guid clientId)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }
        return MapToResponseWithDetails(contract);
    }

    public async Task<IEnumerable<ContractResponse>> GetByCompanyAsync(Guid companyId, Guid clientId)
    {
        var contracts = await _contractRepository.GetByCompanyAsync(companyId, clientId);
        return contracts.Select(MapToResponse);
    }

    public async Task<IEnumerable<ContractResponse>> GetByStatusAsync(Guid clientId, ContractStatus status)
    {
        var contracts = await _contractRepository.GetByStatusAsync(clientId, status);
        return contracts.Select(MapToResponse);
    }

    public async Task<IEnumerable<ContractResponse>> GetExpiredContractsAsync(Guid clientId)
    {
        var contracts = await _contractRepository.GetExpiredContractsAsync(clientId);
        return contracts.Select(MapToResponse);
    }

    public async Task<ContractResponse> CreateAsync(
        Guid clientId,
        CreateContractRequest request,
        Guid? userId = null)
    {
        // Verify company exists and belongs to client
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);
        if (company == null || company.ClientId != clientId)
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        // Verify template if specified
        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId.Value, clientId);
            if (template == null)
            {
                throw new NotFoundException("ContractTemplate", request.TemplateId.Value);
            }

            // Contract type should match template type
            if (template.TemplateType != request.ContractType)
            {
                throw new ValidationException(
                    "ContractType",
                    $"Tipo de contrato não corresponde ao tipo do template. " +
                    $"Esperado: {template.TemplateType}, Recebido: {request.ContractType}");
            }
        }

        // Create entity
        var contract = Contract.Create(
            clientId,
            request.CompanyId,
            request.Title,
            request.ContractType,
            request.TemplateId,
            request.Description,
            null, // contractDate
            request.ExpirationDate,
            userId);

        await _contractRepository.AddAsync(contract);

        return MapToResponse(contract);
    }

    public async Task<ContractResponse> UpdateAsync(
        Guid id,
        Guid clientId,
        UpdateContractRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetByIdAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        // Cannot update executed or cancelled contracts
        if (contract.Status == ContractStatus.Executed || contract.Status == ContractStatus.Cancelled)
        {
            throw new ValidationException("Status", "Não é possível editar contratos executados ou cancelados");
        }

        // Update properties
        var titleProperty = typeof(Contract).GetProperty(nameof(Contract.Title));
        titleProperty?.SetValue(contract, request.Title);

        var expirationProperty = typeof(Contract).GetProperty(nameof(Contract.ExpirationDate));
        expirationProperty?.SetValue(contract, request.ExpirationDate);

        var descriptionProperty = typeof(Contract).GetProperty(nameof(Contract.Description));
        descriptionProperty?.SetValue(contract, request.Description);

        // Update audit fields
        var updatedAtProperty = typeof(Contract).GetProperty(nameof(Contract.UpdatedAt));
        updatedAtProperty?.SetValue(contract, DateTime.UtcNow);

        var updatedByProperty = typeof(Contract).GetProperty(nameof(Contract.UpdatedBy));
        updatedByProperty?.SetValue(contract, userId);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponse(contract);
    }

    public async Task<ContractResponse> UpdateStatusAsync(
        Guid id,
        Guid clientId,
        UpdateContractStatusRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        // Apply status transition based on requested status
        switch (request.Status)
        {
            case ContractStatus.PendingReview:
                contract.SubmitForReview(userId);
                break;
            
            case ContractStatus.Approved:
                contract.Approve(userId);
                break;
            
            case ContractStatus.SentForSignature:
                contract.SendForSignature(userId);
                break;
            
            case ContractStatus.Executed:
                contract.MarkAsExecuted(userId);
                break;
            
            case ContractStatus.Cancelled:
                contract.Cancel(request.Reason, userId);
                break;
            
            default:
                throw new ValidationException("Status", $"Transição de status para '{request.Status}' não é suportada diretamente");
        }

        await _contractRepository.UpdateAsync(contract);

        return MapToResponse(contract);
    }

    public async Task<ContractResponse> AttachDocumentAsync(
        Guid id,
        Guid clientId,
        AttachDocumentRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetByIdAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        contract.SetDocument(request.DocumentPath, request.DocumentSize, request.DocumentHash, userId);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponse(contract);
    }

    public async Task<ContractResponse> AddPartyAsync(
        Guid id,
        Guid clientId,
        AddContractPartyRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        // Cannot add parties to executed or cancelled contracts
        if (contract.Status == ContractStatus.Executed || contract.Status == ContractStatus.Cancelled)
        {
            throw new ValidationException("Status", "Não é possível adicionar partes a contratos executados ou cancelados");
        }

        // Create party entity
        var party = ContractParty.Create(
            id,
            request.PartyName,
            request.PartyEmail,
            request.PartyType,
            request.UserId,
            request.ShareholderId,
            request.SequenceOrder);

        contract.AddParty(party);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> AddClauseAsync(
        Guid id,
        Guid clientId,
        AddContractClauseRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        // Verify clause exists
        var clause = await _clauseRepository.GetByIdAsync(request.ClauseId, clientId);
        if (clause == null)
        {
            throw new NotFoundException("Clause", request.ClauseId);
        }

        // Cannot add clauses to executed or cancelled contracts
        if (contract.Status == ContractStatus.Executed || contract.Status == ContractStatus.Cancelled)
        {
            throw new ValidationException("Status", "Não é possível adicionar cláusulas a contratos executados ou cancelados");
        }

        // Create contract clause entity
        var contractClause = ContractClause.Create(
            id,
            request.ClauseId,
            request.DisplayOrder,
            request.IsMandatory,
            request.ClauseVariables,
            request.CustomContent,
            request.Notes,
            userId);

        contract.AddClause(contractClause);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> RemovePartyAsync(
        Guid id,
        Guid clientId,
        Guid partyId,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        var party = contract.Parties?.FirstOrDefault(p => p.Id == partyId);
        if (party == null)
        {
            throw new NotFoundException("ContractParty", partyId);
        }

        // Cannot modify parties on executed contracts
        if (contract.Status == ContractStatus.Executed)
        {
            throw new ValidationException("Status", "Não é possível remover partes de contratos executados");
        }

        contract.RemoveParty(partyId);
        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> UpdatePartyAsync(
        Guid id,
        Guid clientId,
        Guid partyId,
        UpdateContractPartyRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        var party = contract.Parties?.FirstOrDefault(p => p.Id == partyId);
        if (party == null)
        {
            throw new NotFoundException("ContractParty", partyId);
        }

        // Use reflection to update party properties
        typeof(ContractParty).GetProperty(nameof(ContractParty.PartyName))?.SetValue(party, request.PartyName);
        typeof(ContractParty).GetProperty(nameof(ContractParty.PartyEmail))?.SetValue(party, request.PartyEmail);
        typeof(ContractParty).GetProperty(nameof(ContractParty.SequenceOrder))?.SetValue(party, request.SequenceOrder);
        typeof(ContractParty).GetProperty(nameof(ContractParty.UpdatedAt))?.SetValue(party, DateTime.UtcNow);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> RemoveClauseAsync(
        Guid id,
        Guid clientId,
        Guid clauseId,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        var clause = contract.Clauses?.FirstOrDefault(c => c.Id == clauseId);
        if (clause == null)
        {
            throw new NotFoundException("ContractClause", clauseId);
        }

        if (clause.IsMandatory)
        {
            throw new ValidationException("Clause", "Não é possível remover cláusulas obrigatórias");
        }

        contract.RemoveClause(clauseId);
        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> UpdateClauseAsync(
        Guid id,
        Guid clientId,
        Guid clauseId,
        UpdateContractClauseRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        var clause = contract.Clauses?.FirstOrDefault(c => c.Id == clauseId);
        if (clause == null)
        {
            throw new NotFoundException("ContractClause", clauseId);
        }

        // Use reflection to update clause properties
        typeof(ContractClause).GetProperty(nameof(ContractClause.DisplayOrder))?.SetValue(clause, request.DisplayOrder);
        typeof(ContractClause).GetProperty(nameof(ContractClause.CustomContent))?.SetValue(clause, request.CustomContent);
        typeof(ContractClause).GetProperty(nameof(ContractClause.ClauseVariables))?.SetValue(clause, request.ClauseVariables);
        typeof(ContractClause).GetProperty(nameof(ContractClause.Notes))?.SetValue(clause, request.Notes);
        typeof(ContractClause).GetProperty(nameof(ContractClause.UpdatedAt))?.SetValue(clause, DateTime.UtcNow);

        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task<ContractResponse> ReorderClausesAsync(
        Guid id,
        Guid clientId,
        ReorderClausesRequest request,
        Guid? userId = null)
    {
        var contract = await _contractRepository.GetWithDetailsAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        if (request.ClauseOrders == null || !request.ClauseOrders.Any())
        {
            throw new ValidationException("ClauseOrders", "Lista de ordem de cláusulas não pode estar vazia");
        }

        foreach (var clauseOrder in request.ClauseOrders)
        {
            var clause = contract.Clauses?.FirstOrDefault(c => c.Id == clauseOrder.ClauseId);
            if (clause != null)
            {
                typeof(ContractClause).GetProperty(nameof(ContractClause.DisplayOrder))?.SetValue(clause, clauseOrder.DisplayOrder);
            }
        }

        await _contractRepository.UpdateAsync(contract);

        return MapToResponseWithDetails(contract);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var contract = await _contractRepository.GetByIdAsync(id, clientId);
        if (contract == null)
        {
            throw new NotFoundException("Contract", id);
        }

        // Cannot delete executed contracts
        if (contract.Status == ContractStatus.Executed)
        {
            throw new ValidationException("Status", "Não é possível excluir contratos executados. Considere cancelar ao invés de excluir.");
        }

        await _contractRepository.SoftDeleteAsync(id, clientId, userId);
    }

    /// <summary>
    /// Map entity to response DTO (basic - without parties/clauses)
    /// </summary>
    private static ContractResponse MapToResponse(Contract contract)
    {
        return new ContractResponse
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            CompanyId = contract.CompanyId,
            CompanyName = string.Empty, // Will be filled by controller if needed
            Title = contract.Title,
            ContractType = contract.ContractType,
            Status = contract.Status,
            TemplateId = contract.TemplateId,
            DocumentPath = contract.DocumentPath,
            DocumentSize = contract.DocumentSize,
            DocumentHash = contract.DocumentHash,
            ExpirationDate = contract.ExpirationDate,
            Description = contract.Description,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            CreatedBy = contract.CreatedBy?.ToString(),
            UpdatedBy = contract.UpdatedBy?.ToString(),
            Parties = new List<ContractPartyResponse>(),
            Clauses = new List<ContractClauseResponse>()
        };
    }

    /// <summary>
    /// Map entity to response DTO with details (parties and clauses)
    /// </summary>
    private static ContractResponse MapToResponseWithDetails(Contract contract)
    {
        var response = MapToResponse(contract);

        response = response with
        {
            Parties = contract.Parties?.Select(p => new ContractPartyResponse
            {
                Id = p.Id,
                ContractId = p.ContractId,
                PartyType = p.PartyType,
                PartyName = p.PartyName,
                PartyEmail = p.PartyEmail,
                UserId = p.UserId,
                ShareholderId = p.ShareholderId,
                SignatureStatus = p.SignatureStatus,
                SignatureDate = p.SignatureDate,
                SignatureToken = p.SignatureToken,
                SignatureExternalId = p.ExternalId, // Corrected property name
                SequenceOrder = p.SequenceOrder,
                RejectionReason = null // Not yet implemented in entity
            }).ToList() ?? new List<ContractPartyResponse>(),

            Clauses = contract.Clauses?.Select(c => new ContractClauseResponse
            {
                Id = c.Id,
                ContractId = c.ContractId,
                ClauseId = c.ClauseId,
                CustomContent = c.CustomContent,
                DisplayOrder = c.DisplayOrder,
                IsMandatory = c.IsMandatory,
                ClauseVariables = c.ClauseVariables ?? new Dictionary<string, string>(),
                Notes = c.Notes,
                // Clause details will be filled by repository if needed
                ClauseName = string.Empty,
                ClauseCode = string.Empty,
                ClauseType = ClauseType.General,
                EffectiveContent = c.GetEffectiveContent()
            }).ToList() ?? new List<ContractClauseResponse>()
        };

        return response;
    }
}
