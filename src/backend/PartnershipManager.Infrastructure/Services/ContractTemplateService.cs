using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Interface for ContractTemplate service operations
/// </summary>
public interface IContractTemplateService
{
    Task<ContractTemplateListResponse> GetPagedAsync(
        Guid clientId, 
        int page, 
        int pageSize, 
        string? search = null,
        ContractTemplateType? type = null, 
        bool? isActive = null);
    
    Task<ContractTemplateResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<ContractTemplateResponse> GetByCodeAsync(string code, Guid clientId);
    Task<IEnumerable<ContractTemplateResponse>> GetActiveTemplatesAsync(Guid clientId);
    Task<IEnumerable<ContractTemplateResponse>> GetByTypeAsync(Guid clientId, ContractTemplateType type);
    Task<ContractTemplateResponse> CreateAsync(Guid clientId, CreateContractTemplateRequest request, Guid? userId = null);
    Task<ContractTemplateResponse> UpdateAsync(Guid id, Guid clientId, UpdateContractTemplateRequest request, Guid? userId = null);
    Task<ContractTemplateResponse> CloneAsync(Guid id, Guid clientId, CloneContractTemplateRequest request, Guid? userId = null);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

/// <summary>
/// Service for managing contract templates
/// </summary>
public class ContractTemplateService : IContractTemplateService
{
    private readonly IContractTemplateRepository _templateRepository;

    public ContractTemplateService(IContractTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<ContractTemplateListResponse> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        ContractTemplateType? type = null,
        bool? isActive = null)
    {
        var (items, total) = await _templateRepository.GetPagedAsync(
            clientId, page, pageSize, search, type?.ToString(), isActive);
        
        var responseItems = items.Select(MapToResponse);
        return new ContractTemplateListResponse(responseItems, total, page, pageSize);
    }

    public async Task<ContractTemplateResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId);
        if (template == null)
        {
            throw new NotFoundException("ContractTemplate", id);
        }
        return MapToResponse(template);
    }

    public async Task<ContractTemplateResponse> GetByCodeAsync(string code, Guid clientId)
    {
        var template = await _templateRepository.GetByCodeAsync(clientId, code);
        if (template == null)
        {
            throw new NotFoundException("ContractTemplate", code);
        }
        return MapToResponse(template);
    }

    public async Task<IEnumerable<ContractTemplateResponse>> GetActiveTemplatesAsync(Guid clientId)
    {
        var templates = await _templateRepository.GetActiveTemplatesAsync(clientId);
        return templates.Select(MapToResponse);
    }

    public async Task<IEnumerable<ContractTemplateResponse>> GetByTypeAsync(
        Guid clientId,
        ContractTemplateType type)
    {
        var templates = await _templateRepository.GetByTypeAsync(clientId, type);
        return templates.Select(MapToResponse);
    }

    public async Task<ContractTemplateResponse> CreateAsync(
        Guid clientId,
        CreateContractTemplateRequest request,
        Guid? userId = null)
    {
        // Verify code doesn't already exist
        var codeExists = await _templateRepository.CodeExistsAsync(clientId, request.Code);
        if (codeExists)
        {
            throw new ConflictException($"Código '{request.Code}' já está em uso");
        }

        // Create entity
        var template = ContractTemplate.Create(
            clientId,
            request.Name,
            request.Code,
            request.TemplateType,
            request.Content,
            request.Description,
            null, // companyId
            request.DefaultStatus,
            request.Tags,
            userId);

        await _templateRepository.AddAsync(template);

        return MapToResponse(template);
    }

    public async Task<ContractTemplateResponse> UpdateAsync(
        Guid id,
        Guid clientId,
        UpdateContractTemplateRequest request,
        Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId);
        if (template == null)
        {
            throw new NotFoundException("ContractTemplate", id);
        }

        // Update content (increments version)
        if (template.Content != request.Content)
        {
            template.UpdateContent(request.Content, userId);
        }

        // Update name and description if changed
        if (template.Name != request.Name)
        {
            var nameProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.Name));
            nameProperty?.SetValue(template, request.Name);
        }

        if (template.Description != request.Description)
        {
            var descProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.Description));
            descProperty?.SetValue(template, request.Description);
        }

        // Update default status
        var statusProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.DefaultStatus));
        statusProperty?.SetValue(template, request.DefaultStatus);

        // Update tags
        var tagsProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.Tags));
        tagsProperty?.SetValue(template, request.Tags?.ToArray() ?? Array.Empty<string>());

        // Update active status
        if (!request.IsActive && template.IsActive)
        {
            template.Deactivate(userId);
        }
        else if (request.IsActive && !template.IsActive)
        {
            var isActiveProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.IsActive));
            isActiveProperty?.SetValue(template, true);
        }

        // Update audit fields
        var updatedAtProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.UpdatedAt));
        updatedAtProperty?.SetValue(template, DateTime.UtcNow);

        var updatedByProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.UpdatedBy));
        updatedByProperty?.SetValue(template, userId);

        await _templateRepository.UpdateAsync(template);

        return MapToResponse(template);
    }

    public async Task<ContractTemplateResponse> CloneAsync(
        Guid id,
        Guid clientId,
        CloneContractTemplateRequest request,
        Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId);
        if (template == null)
        {
            throw new NotFoundException("ContractTemplate", id);
        }

        // Verify new code doesn't exist
        var codeExists = await _templateRepository.CodeExistsAsync(clientId, request.NewCode);
        if (codeExists)
        {
            throw new ConflictException($"Código '{request.NewCode}' já está em uso");
        }

        // Clone template
        var clonedTemplate = template.Clone(request.NewCode, userId);

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.NewName))
        {
            var nameProperty = typeof(ContractTemplate).GetProperty(nameof(ContractTemplate.Name));
            nameProperty?.SetValue(clonedTemplate, request.NewName);
        }

        await _templateRepository.AddAsync(clonedTemplate);

        return MapToResponse(clonedTemplate);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId);
        if (template == null)
        {
            throw new NotFoundException("ContractTemplate", id);
        }

        await _templateRepository.SoftDeleteAsync(id, clientId, userId);
    }

    /// <summary>
    /// Map entity to response DTO
    /// </summary>
    private static ContractTemplateResponse MapToResponse(ContractTemplate template)
    {
        return new ContractTemplateResponse
        {
            Id = template.Id,
            ClientId = template.ClientId,
            Name = template.Name,
            Code = template.Code,
            TemplateType = template.TemplateType,
            Content = template.Content,
            DefaultStatus = template.DefaultStatus,
            Version = template.Version,
            IsActive = template.IsActive,
            Tags = template.Tags?.ToList() ?? new List<string>(),
            Description = template.Description,
            Variables = template.ExtractVariables().ToList(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy?.ToString(),
            UpdatedBy = template.UpdatedBy?.ToString()
        };
    }
}
