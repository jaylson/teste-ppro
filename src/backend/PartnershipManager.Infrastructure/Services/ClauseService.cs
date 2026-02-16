using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Interface for Clause service operations
/// </summary>
public interface IClauseService
{
    Task<ClauseListResponse> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        ClauseType? clauseType = null,
        bool? isMandatory = null,
        bool? isActive = null);
    
    Task<ClauseResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<ClauseResponse> GetByCodeAsync(string code, Guid clientId);
    Task<IEnumerable<ClauseResponse>> GetActiveClausesAsync(Guid clientId);
    Task<IEnumerable<ClauseResponse>> GetMandatoryClausesAsync(Guid clientId);
    Task<IEnumerable<ClauseResponse>> GetByTypeAsync(Guid clientId, ClauseType type);
    Task<ClauseResponse> CreateAsync(Guid clientId, CreateClauseRequest request, Guid? userId = null);
    Task<ClauseResponse> UpdateAsync(Guid id, Guid clientId, UpdateClauseRequest request, Guid? userId = null);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

/// <summary>
/// Service for managing contract clauses
/// </summary>
public class ClauseService : IClauseService
{
    private readonly IClauseRepository _clauseRepository;

    public ClauseService(IClauseRepository clauseRepository)
    {
        _clauseRepository = clauseRepository;
    }

    public async Task<ClauseListResponse> GetPagedAsync(
        Guid clientId,
        int page,
        int pageSize,
        string? search = null,
        ClauseType? clauseType = null,
        bool? isMandatory = null,
        bool? isActive = null)
    {
        var (items, total) = await _clauseRepository.GetPagedAsync(
            clientId, page, pageSize, search, clauseType?.ToString(), isMandatory, isActive);
        
       var responseItems = items.Select(MapToResponse);
        return new ClauseListResponse(responseItems, total, page, pageSize);
    }

    public async Task<ClauseResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var clause = await _clauseRepository.GetByIdAsync(id, clientId);
        if (clause == null)
        {
            throw new NotFoundException("Clause", id);
        }
        return MapToResponse(clause);
    }

    public async Task<ClauseResponse> GetByCodeAsync(string code, Guid clientId)
    {
        var clause = await _clauseRepository.GetByCodeAsync(clientId, code);
        if (clause == null)
        {
            throw new NotFoundException("Clause", code);
        }
        return MapToResponse(clause);
    }

    public async Task<IEnumerable<ClauseResponse>> GetActiveClausesAsync(Guid clientId)
    {
        var clauses = await _clauseRepository.GetActiveClausesAsync(clientId);
        return clauses.Select(MapToResponse);
    }

    public async Task<IEnumerable<ClauseResponse>> GetMandatoryClausesAsync(Guid clientId)
    {
        var clauses = await _clauseRepository.GetMandatoryClausesAsync(clientId);
        return clauses.Select(MapToResponse);
    }

    public async Task<IEnumerable<ClauseResponse>> GetByTypeAsync(Guid clientId, ClauseType type)
    {
        var clauses = await _clauseRepository.GetByTypeAsync(clientId, type);
        return clauses.Select(MapToResponse);
    }

    public async Task<ClauseResponse> CreateAsync(
        Guid clientId,
        CreateClauseRequest request,
        Guid? userId = null)
    {
        // Verify code doesn't already exist
        var codeExists = await _clauseRepository.CodeExistsAsync(clientId, request.Code);
        if (codeExists)
        {
            throw new ConflictException($"Código '{request.Code}' já está em uso");
        }

        // Create entity
        var clause = Clause.Create(
            clientId,
            request.Name,
            request.Code,
            request.Content,
            request.ClauseType,
            request.IsMandatory,
            request.Description,
            request.Tags,
            request.DisplayOrder,
            userId);

        await _clauseRepository.AddAsync(clause);

        return MapToResponse(clause);
    }

    public async Task<ClauseResponse> UpdateAsync(
        Guid id,
        Guid clientId,
        UpdateClauseRequest request,
        Guid? userId = null)
    {
        var clause = await _clauseRepository.GetByIdAsync(id, clientId);
        if (clause == null)
        {
            throw new NotFoundException("Clause", id);
        }

        // Update metadata (name, description, tags, mandatory, display order)
        clause.UpdateMetadata(
            name: request.Name,
            description: request.Description,
            tags: request.Tags,
            isMandatory: request.IsMandatory,
            displayOrder: request.DisplayOrder,
            isActive: request.IsActive,
            updatedBy: userId);

        // Update content if changed (increments version)
        if (clause.Content != request.Content)
        {
            var contentProperty = typeof(Clause).GetProperty(nameof(Clause.Content));
            contentProperty?.SetValue(clause, request.Content);
            
            var versionProperty = typeof(Clause).GetProperty(nameof(Clause.Version));
            versionProperty?.SetValue(clause, clause.Version + 1);
        }

        // Update active status
        if (!request.IsActive && clause.IsActive)
        {
            clause.Deactivate(userId);
        }
        else if (request.IsActive && !clause.IsActive)
        {
            var isActiveProperty = typeof(Clause).GetProperty(nameof(Clause.IsActive));
            isActiveProperty?.SetValue(clause, true);
        }

        // Update audit fields
        var updatedAtProperty = typeof(Clause).GetProperty(nameof(Clause.UpdatedAt));
        updatedAtProperty?.SetValue(clause, DateTime.UtcNow);

        var updatedByProperty = typeof(Clause).GetProperty(nameof(Clause.UpdatedBy));
        updatedByProperty?.SetValue(clause, userId);

        await _clauseRepository.UpdateAsync(clause);

        return MapToResponse(clause);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var clause = await _clauseRepository.GetByIdAsync(id, clientId);
        if (clause == null)
        {
            throw new NotFoundException("Clause", id);
        }

        await _clauseRepository.SoftDeleteAsync(id, clientId, userId);
    }

    /// <summary>
    /// Map entity to response DTO
    /// </summary>
    private static ClauseResponse MapToResponse(Clause clause)
    {
        return new ClauseResponse
        {
            Id = clause.Id,
            ClientId = clause.ClientId,
            Name = clause.Name,
            Code = clause.Code,
            ClauseType = clause.ClauseType,
            IsMandatory = clause.IsMandatory,
            DisplayOrder = clause.DisplayOrder,
            Version = clause.Version,
            Content = clause.Content,
            IsActive = clause.IsActive,
            Tags = clause.Tags?.ToList() ?? new List<string>(),
            Description = clause.Description,
            Variables = clause.ExtractVariables().ToList(),
            CreatedAt = clause.CreatedAt,
            UpdatedAt = clause.UpdatedAt,
            CreatedBy = clause.CreatedBy?.ToString(),
            UpdatedBy = clause.UpdatedBy?.ToString()
        };
    }
}
