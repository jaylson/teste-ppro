using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly DapperContext _context;

    public NotificationRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateAsync(Notification n)
    {
        var sql = @"
            INSERT INTO notifications (id, user_id, company_id, notification_type, title, body,
                action_url, reference_type, reference_id, is_read, read_at, created_at, updated_at)
            VALUES (@Id, @UserId, @CompanyId, @NotificationType, @Title, @Body,
                @ActionUrl, @ReferenceType, @ReferenceId, @IsRead, @ReadAt, @CreatedAt, @UpdatedAt)";
        await _context.Connection.ExecuteAsync(sql, n);
        return n.Id;
    }

    public async Task<(IEnumerable<Notification> Items, int Total)> GetByUserAsync(
        Guid userId, Guid? companyId, int page, int pageSize)
    {
        var companyFilter = companyId.HasValue ? "AND company_id = @CompanyId" : string.Empty;
        var parameters = new { UserId = userId, CompanyId = companyId, PageSize = pageSize, Offset = (page - 1) * pageSize };

        var total = await _context.Connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM notifications WHERE user_id = @UserId {companyFilter} AND deleted_at IS NULL",
            parameters);

        var sql = $@"
            SELECT id AS Id, user_id AS UserId, company_id AS CompanyId,
                   notification_type AS NotificationType, title AS Title, body AS Body,
                   action_url AS ActionUrl, reference_type AS ReferenceType,
                   reference_id AS ReferenceId, is_read AS IsRead, read_at AS ReadAt,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM notifications
            WHERE user_id = @UserId {companyFilter} AND deleted_at IS NULL
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await _context.Connection.QueryAsync<Notification>(sql, parameters);
        return (items, total);
    }

    public async Task<IEnumerable<Notification>> GetRecentByUserAsync(Guid userId, Guid? companyId, int limit = 10)
    {
        var companyFilter = companyId.HasValue ? "AND company_id = @CompanyId" : string.Empty;
        var sql = $@"
            SELECT id AS Id, user_id AS UserId, company_id AS CompanyId,
                   notification_type AS NotificationType, title AS Title, body AS Body,
                   action_url AS ActionUrl, reference_type AS ReferenceType,
                   reference_id AS ReferenceId, is_read AS IsRead, read_at AS ReadAt,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM notifications
            WHERE user_id = @UserId {companyFilter} AND deleted_at IS NULL
            ORDER BY created_at DESC LIMIT @Limit";
        return await _context.Connection.QueryAsync<Notification>(sql, new { UserId = userId, CompanyId = companyId, Limit = limit });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, Guid? companyId)
    {
        var companyFilter = companyId.HasValue ? "AND company_id = @CompanyId" : string.Empty;
        var sql = $"SELECT COUNT(*) FROM notifications WHERE user_id = @UserId {companyFilter} AND is_read = 0 AND deleted_at IS NULL";
        return await _context.Connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, CompanyId = companyId });
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId)
    {
        var sql = @"UPDATE notifications SET is_read = 1, read_at = @Now, updated_at = @Now
                    WHERE id = @Id AND user_id = @UserId AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, new { Id = id, UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task MarkAllAsReadAsync(Guid userId, Guid companyId)
    {
        var sql = @"UPDATE notifications SET is_read = 1, read_at = @Now, updated_at = @Now
                    WHERE user_id = @UserId AND company_id = @CompanyId AND is_read = 0 AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, new { UserId = userId, CompanyId = companyId, Now = DateTime.UtcNow });
    }

    public async Task<NotificationPreference?> GetPreferenceAsync(Guid userId, string notificationType)
    {
        var sql = @"SELECT id AS Id, user_id AS UserId, notification_type AS NotificationType,
                           channel AS Channel, created_at AS CreatedAt, updated_at AS UpdatedAt
                    FROM notification_preferences
                    WHERE user_id = @UserId AND notification_type = @NotificationType AND deleted_at IS NULL";
        return await _context.Connection.QueryFirstOrDefaultAsync<NotificationPreference>(sql,
            new { UserId = userId, NotificationType = notificationType });
    }

    public async Task UpsertPreferenceAsync(NotificationPreference p)
    {
        if (p.Id == Guid.Empty) p.Id = Guid.NewGuid();
        p.CreatedAt = DateTime.UtcNow;
        p.UpdatedAt = DateTime.UtcNow;

        // Atomic upsert using MySQL ON DUPLICATE KEY UPDATE
        // Requires a UNIQUE KEY on (user_id, notification_type)
        var sql = @"
            INSERT INTO notification_preferences (id, user_id, notification_type, channel, created_at, updated_at)
            VALUES (@Id, @UserId, @NotificationType, @Channel, @CreatedAt, @UpdatedAt)
            ON DUPLICATE KEY UPDATE channel = VALUES(channel), updated_at = VALUES(updated_at)";
        await _context.Connection.ExecuteAsync(sql, p);
    }

    public async Task<IEnumerable<NotificationPreference>> GetAllPreferencesAsync(Guid userId)
    {
        var sql = @"SELECT id AS Id, user_id AS UserId, notification_type AS NotificationType,
                           channel AS Channel, created_at AS CreatedAt, updated_at AS UpdatedAt
                    FROM notification_preferences WHERE user_id = @UserId AND deleted_at IS NULL";
        return await _context.Connection.QueryAsync<NotificationPreference>(sql, new { UserId = userId });
    }
}
