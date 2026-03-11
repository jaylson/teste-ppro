using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Communication;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Application.Services;

public interface ICommunicationService
{
    Task<PagedResult<CommunicationListResponse>> GetByCompanyAsync(Guid companyId, Guid? userId, int page, int pageSize, string? search = null, string? commType = null, bool? isPublished = null);
    Task<CommunicationResponse?> GetByIdAsync(Guid id, Guid companyId, Guid? userId = null);
    Task<Guid> CreateAsync(Guid companyId, CreateCommunicationRequest request, Guid userId);
    Task UpdateAsync(Guid id, Guid companyId, UpdateCommunicationRequest request, Guid userId);
    Task PublishAsync(Guid id, Guid companyId, Guid userId);
    Task DeleteAsync(Guid id, Guid companyId, Guid userId);
    Task TrackViewAsync(Guid id, Guid userId, int? durationSecs);
    Task<IEnumerable<CommunicationListResponse>> GetForPortalAsync(Guid companyId, string role, int limit = 10);
}

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _repo;

    public CommunicationService(ICommunicationRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<CommunicationListResponse>> GetByCompanyAsync(
        Guid companyId, Guid? userId, int page, int pageSize,
        string? search = null, string? commType = null, bool? isPublished = null)
    {
        var (items, total) = await _repo.GetByCompanyAsync(companyId, page, pageSize, search, commType, isPublished);
        var mapped = items.Select(c => MapToListResponse(c));
        return new PagedResult<CommunicationListResponse>(mapped, total, page, pageSize);
    }

    public async Task<CommunicationResponse?> GetByIdAsync(Guid id, Guid companyId, Guid? userId = null)
    {
        var c = await _repo.GetByIdAsync(id, companyId);
        if (c == null) return null;

        bool hasViewed = userId.HasValue && await _repo.HasViewedAsync(id, userId.Value);
        return MapToResponse(c, hasViewed);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateCommunicationRequest request, Guid userId)
    {
        var c = new Communication
        {
            CompanyId = companyId,
            Title = request.Title,
            Content = request.Content,
            ContentHtml = request.ContentHtml,
            Summary = request.Summary,
            CommType = request.CommType,
            Visibility = request.Visibility,
            TargetRoles = request.TargetRoles,
            IsPinned = request.IsPinned,
            ExpiresAt = request.ExpiresAt
        };
        c.CreatedBy = userId;
        c.UpdatedBy = userId;
        return await _repo.CreateAsync(c);
    }

    public async Task UpdateAsync(Guid id, Guid companyId, UpdateCommunicationRequest request, Guid userId)
    {
        var c = await _repo.GetByIdAsync(id, companyId)
            ?? throw new InvalidOperationException("Comunicação não encontrada.");

        c.Title = request.Title;
        c.Content = request.Content;
        c.ContentHtml = request.ContentHtml;
        c.Summary = request.Summary;
        c.CommType = request.CommType;
        c.Visibility = request.Visibility;
        c.TargetRoles = request.TargetRoles;
        c.IsPinned = request.IsPinned;
        c.ExpiresAt = request.ExpiresAt;
        c.UpdatedBy = userId;

        await _repo.UpdateAsync(c);
    }

    public Task PublishAsync(Guid id, Guid companyId, Guid userId)
        => _repo.PublishAsync(id, companyId);

    public Task DeleteAsync(Guid id, Guid companyId, Guid userId)
        => _repo.SoftDeleteAsync(id, companyId);

    public Task TrackViewAsync(Guid id, Guid userId, int? durationSecs)
        => _repo.TrackViewAsync(id, userId, durationSecs);

    public async Task<IEnumerable<CommunicationListResponse>> GetForPortalAsync(Guid companyId, string role, int limit = 10)
    {
        var items = await _repo.GetForRoleAsync(companyId, role, limit);
        return items.Select(c => MapToListResponse(c));
    }

    private static CommunicationListResponse MapToListResponse(Communication c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        CommType = c.CommType,
        Visibility = c.Visibility,
        IsPinned = c.IsPinned,
        PublishedAt = c.PublishedAt,
        CreatedAt = c.CreatedAt,
        ViewsCount = c.ViewsCount,
        Summary = c.Summary
    };

    private static CommunicationResponse MapToResponse(Communication c, bool hasViewed) => new()
    {
        Id = c.Id,
        CompanyId = c.CompanyId,
        Title = c.Title,
        Content = c.Content,
        ContentHtml = c.ContentHtml,
        Summary = c.Summary,
        CommType = c.CommType,
        Visibility = c.Visibility,
        TargetRoles = c.TargetRoles,
        IsPinned = c.IsPinned,
        PublishedAt = c.PublishedAt,
        ExpiresAt = c.ExpiresAt,
        ViewsCount = c.ViewsCount,
        HasViewed = hasViewed,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
