using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly DapperContext _context;

    public CommunicationRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Communication> Items, int Total)> GetByCompanyAsync(
        Guid companyId, int page, int pageSize,
        string? search = null, string? commType = null, bool? isPublished = null)
    {
        var conditions = new List<string> { "company_id = @CompanyId", "deleted_at IS NULL" };
        var parameters = new DynamicParameters();
        parameters.Add("CompanyId", companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add("(title LIKE @Search OR summary LIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }
        if (!string.IsNullOrWhiteSpace(commType))
        {
            conditions.Add("comm_type = @CommType");
            parameters.Add("CommType", commType);
        }
        if (isPublished.HasValue)
        {
            conditions.Add(isPublished.Value ? "published_at IS NOT NULL" : "published_at IS NULL");
        }

        var where = string.Join(" AND ", conditions);
        var offset = (page - 1) * pageSize;

        var countSql = $"SELECT COUNT(*) FROM communications WHERE {where}";
        var total = await _context.Connection.ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT id AS Id, company_id AS CompanyId, title AS Title, content AS Content,
                   content_html AS ContentHtml, summary AS Summary, comm_type AS CommType,
                   visibility AS Visibility, target_roles AS TargetRoles, send_email AS SendEmail,
                   attachments AS Attachments,
                   is_pinned AS IsPinned, published_at AS PublishedAt, expires_at AS ExpiresAt,
                   created_by AS CreatedBy, views_count AS ViewsCount,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM communications
            WHERE {where}
            ORDER BY is_pinned DESC, created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        var items = await _context.Connection.QueryAsync<Communication>(sql, parameters);
        return (items, total);
    }

    public async Task<Communication?> GetByIdAsync(Guid id, Guid companyId)
    {
        var sql = @"
            SELECT id AS Id, company_id AS CompanyId, title AS Title, content AS Content,
                   content_html AS ContentHtml, summary AS Summary, comm_type AS CommType,
                   visibility AS Visibility, target_roles AS TargetRoles, send_email AS SendEmail,
                   attachments AS Attachments,
                   is_pinned AS IsPinned, published_at AS PublishedAt, expires_at AS ExpiresAt,
                   created_by AS CreatedBy, views_count AS ViewsCount,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM communications
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL";
        return await _context.Connection.QueryFirstOrDefaultAsync<Communication>(sql, new { Id = id, CompanyId = companyId });
    }

    public async Task<Guid> CreateAsync(Communication c)
    {
        var sql = @"
            INSERT INTO communications
                (id, company_id, title, content, content_html, summary, comm_type, visibility,
                 target_roles, send_email, attachments, is_pinned, published_at, expires_at, created_by,
                 views_count, created_at, updated_at, updated_by)
            VALUES
                (@Id, @CompanyId, @Title, @Content, @ContentHtml, @Summary, @CommType, @Visibility,
                 @TargetRoles, @SendEmail, @Attachments, @IsPinned, @PublishedAt, @ExpiresAt, @CreatedBy,
                 @ViewsCount, @CreatedAt, @UpdatedAt, @UpdatedBy)";

        await _context.Connection.ExecuteAsync(sql, c);
        return c.Id;
    }

    public async Task UpdateAsync(Communication c)
    {
        c.UpdatedAt = DateTime.UtcNow;
        var sql = @"
            UPDATE communications
            SET title = @Title, content = @Content, content_html = @ContentHtml, summary = @Summary,
                comm_type = @CommType, visibility = @Visibility, target_roles = @TargetRoles,
                send_email = @SendEmail, attachments = @Attachments, is_pinned = @IsPinned,
                expires_at = @ExpiresAt, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, c);
    }

    public async Task SoftDeleteAsync(Guid id, Guid companyId)
    {
        var sql = @"
            UPDATE communications
            SET deleted_at = @Now, updated_at = @Now
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, new { Id = id, CompanyId = companyId, Now = DateTime.UtcNow });
    }

    public async Task PublishAsync(Guid id, Guid companyId)
    {
        var sql = @"
            UPDATE communications
            SET published_at = @Now, updated_at = @Now
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL AND published_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, new { Id = id, CompanyId = companyId, Now = DateTime.UtcNow });
    }

    public async Task TrackViewAsync(Guid communicationId, Guid userId, int? durationSecs)
    {
        using var tx = await _context.BeginTransactionAsync();
        try
        {
            var existing = await _context.Connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM communication_views WHERE communication_id = @CId AND user_id = @UId",
                new { CId = communicationId, UId = userId }, tx);

            if (existing == 0)
            {
                await _context.Connection.ExecuteAsync(
                    @"INSERT INTO communication_views (id, communication_id, user_id, viewed_at, view_duration_secs, created_at, updated_at)
                      VALUES (@Id, @CId, @UId, @Now, @Dur, @Now, @Now)",
                    new { Id = Guid.NewGuid(), CId = communicationId, UId = userId, Now = DateTime.UtcNow, Dur = durationSecs }, tx);

                await _context.Connection.ExecuteAsync(
                    "UPDATE communications SET views_count = views_count + 1, updated_at = @Now WHERE id = @Id",
                    new { Id = communicationId, Now = DateTime.UtcNow }, tx);
            }

            _context.CommitTransaction();
        }
        catch
        {
            _context.RollbackTransaction();
            throw;
        }
    }

    public async Task<bool> HasViewedAsync(Guid communicationId, Guid userId)
    {
        var sql = "SELECT EXISTS(SELECT 1 FROM communication_views WHERE communication_id = @CId AND user_id = @UId)";
        var exists = await _context.Connection.ExecuteScalarAsync<bool>(sql, new { CId = communicationId, UId = userId });
        return exists;
    }

    public async Task<IEnumerable<Communication>> GetForRoleAsync(Guid companyId, string role, int limit)
    {
        var sql = @"
            SELECT id AS Id, company_id AS CompanyId, title AS Title, content AS Content,
                   content_html AS ContentHtml, summary AS Summary, comm_type AS CommType,
                   visibility AS Visibility, target_roles AS TargetRoles, send_email AS SendEmail,
                   attachments AS Attachments,
                   is_pinned AS IsPinned, published_at AS PublishedAt, expires_at AS ExpiresAt,
                   created_by AS CreatedBy, views_count AS ViewsCount,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM communications
            WHERE company_id = @CompanyId AND deleted_at IS NULL AND published_at IS NOT NULL
              AND (visibility = 'all' OR visibility = @Role OR (visibility = 'specific' AND FIND_IN_SET(@Role, target_roles) > 0))
              AND (expires_at IS NULL OR expires_at > @Now)
            ORDER BY is_pinned DESC, published_at DESC
            LIMIT @Limit";
        return await _context.Connection.QueryAsync<Communication>(sql,
            new { CompanyId = companyId, Role = role, Now = DateTime.UtcNow, Limit = limit });
    }
}
