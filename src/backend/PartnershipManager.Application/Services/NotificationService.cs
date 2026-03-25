using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Notification;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Application.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid companyId, Guid userId, string type, string title, string body,
        string? actionUrl = null, string? referenceType = null, Guid? referenceId = null);
    Task NotifyUsersAsync(Guid companyId, IEnumerable<Guid> userIds, string type, string title, string body,
        string? actionUrl = null, string? referenceType = null, Guid? referenceId = null);
    Task<PagedResult<NotificationResponse>> GetByUserAsync(Guid userId, Guid companyId, int page, int pageSize);
    Task<IEnumerable<NotificationResponse>> GetRecentAsync(Guid userId, Guid companyId, int limit = 10);
    Task<int> GetUnreadCountAsync(Guid userId, Guid companyId);
    Task MarkAsReadAsync(Guid id, Guid userId);
    Task MarkAllAsReadAsync(Guid userId, Guid companyId);
    Task<IEnumerable<NotificationPreferenceResponse>> GetPreferencesAsync(Guid userId);
    Task UpdatePreferenceAsync(Guid userId, string notificationType, string channel);
    /// <summary>Returns the notification channel for a user/type ("in_app", "email", "both", "none"). Defaults to "both" when no preference is set.</summary>
    Task<string> GetPreferenceChannelAsync(Guid userId, string notificationType);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public Task NotifyAsync(Guid companyId, Guid userId, string type, string title, string body,
        string? actionUrl = null, string? referenceType = null, Guid? referenceId = null)
    {
        var n = new Notification
        {
            UserId = userId,
            CompanyId = companyId,
            NotificationType = type,
            Title = title,
            Body = body,
            ActionUrl = actionUrl,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            IsRead = false
        };
        return _repo.CreateAsync(n);
    }

    public async Task NotifyUsersAsync(Guid companyId, IEnumerable<Guid> userIds, string type, string title, string body,
        string? actionUrl = null, string? referenceType = null, Guid? referenceId = null)
    {
        foreach (var uid in userIds)
            await NotifyAsync(companyId, uid, type, title, body, actionUrl, referenceType, referenceId);
    }

    public async Task<PagedResult<NotificationResponse>> GetByUserAsync(Guid userId, Guid companyId, int page, int pageSize)
    {
        var (items, total) = await _repo.GetByUserAsync(userId, companyId, page, pageSize);
        return new PagedResult<NotificationResponse>(items.Select(Map), total, page, pageSize);
    }

    public async Task<IEnumerable<NotificationResponse>> GetRecentAsync(Guid userId, Guid companyId, int limit = 10)
    {
        var items = await _repo.GetRecentByUserAsync(userId, companyId, limit);
        return items.Select(Map);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, Guid companyId)
        => _repo.GetUnreadCountAsync(userId, companyId);

    public Task MarkAsReadAsync(Guid id, Guid userId)
        => _repo.MarkAsReadAsync(id, userId);

    public Task MarkAllAsReadAsync(Guid userId, Guid companyId)
        => _repo.MarkAllAsReadAsync(userId, companyId);

    public async Task<IEnumerable<NotificationPreferenceResponse>> GetPreferencesAsync(Guid userId)
    {
        var prefs = await _repo.GetAllPreferencesAsync(userId);
        return prefs.Select(p => new NotificationPreferenceResponse { NotificationType = p.NotificationType, Channel = p.Channel });
    }

    public Task UpdatePreferenceAsync(Guid userId, string notificationType, string channel)
    {
        var p = new NotificationPreference { UserId = userId, NotificationType = notificationType, Channel = channel };
        return _repo.UpsertPreferenceAsync(p);
    }

    public async Task<string> GetPreferenceChannelAsync(Guid userId, string notificationType)
    {
        var pref = await _repo.GetPreferenceAsync(userId, notificationType);
        return pref?.Channel ?? "both";
    }

    private static NotificationResponse Map(Notification n) => new()
    {
        Id = n.Id,
        NotificationType = n.NotificationType,
        Title = n.Title,
        Body = n.Body,
        ActionUrl = n.ActionUrl,
        ReferenceType = n.ReferenceType,
        ReferenceId = n.ReferenceId,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}
