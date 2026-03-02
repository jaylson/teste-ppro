using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class ValuationRepository : IValuationRepository
{
    private readonly DapperContext _context;

    public ValuationRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Valuation> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? status = null, string? eventType = null)
    {
        var where = "WHERE v.client_id = @ClientId AND v.company_id = @CompanyId AND v.is_deleted = 0";
        if (!string.IsNullOrWhiteSpace(status)) where += " AND v.status = @Status";
        if (!string.IsNullOrWhiteSpace(eventType)) where += " AND v.event_type = @EventType";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                v.id, v.client_id, v.company_id,
                v.valuation_date, v.event_type, v.event_name,
                v.valuation_amount, v.total_shares, v.price_per_share,
                v.status, v.notes,
                v.submitted_at, v.submitted_by, v.approved_at, v.approved_by,
                v.rejected_at, v.rejected_by, v.rejection_reason,
                v.created_at, v.updated_at, v.created_by, v.updated_by,
                v.is_deleted, v.deleted_at
            FROM valuations v
            {where}
            ORDER BY v.valuation_date DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Status = status,
            EventType = eventType,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToValuation), total);
    }

    public async Task<Valuation?> GetByIdAsync(Guid id, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, company_id,
                valuation_date, event_type, event_name,
                valuation_amount, total_shares, price_per_share,
                status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                rejected_at, rejected_by, rejection_reason,
                created_at, updated_at, created_by, updated_by,
                is_deleted, deleted_at
            FROM valuations
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToValuation(row);
    }

    public async Task<Valuation?> GetLastApprovedAsync(Guid clientId, Guid companyId)
    {
        const string sql = @"
            SELECT id, client_id, company_id,
                valuation_date, event_type, event_name,
                valuation_amount, total_shares, price_per_share,
                status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                rejected_at, rejected_by, rejection_reason,
                created_at, updated_at, created_by, updated_by,
                is_deleted, deleted_at
            FROM valuations
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND status = 'approved' AND is_deleted = 0
            ORDER BY valuation_date DESC
            LIMIT 1";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToValuation(row);
    }

    public async Task AddAsync(Valuation v)
    {
        const string sql = @"
            INSERT INTO valuations (
                id, client_id, company_id,
                valuation_date, event_type, event_name,
                valuation_amount, total_shares, price_per_share,
                status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                rejected_at, rejected_by, rejection_reason,
                created_at, updated_at, created_by, updated_by,
                is_deleted
            ) VALUES (
                @Id, @ClientId, @CompanyId,
                @ValuationDate, @EventType, @EventName,
                @ValuationAmount, @TotalShares, @PricePerShare,
                @Status, @Notes,
                @SubmittedAt, @SubmittedBy, @ApprovedAt, @ApprovedBy,
                @RejectedAt, @RejectedBy, @RejectionReason,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy,
                0
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(v), _context.Transaction);
    }

    public async Task UpdateAsync(Valuation v)
    {
        const string sql = @"
            UPDATE valuations SET
                valuation_date = @ValuationDate, event_type = @EventType, event_name = @EventName,
                valuation_amount = @ValuationAmount, total_shares = @TotalShares, price_per_share = @PricePerShare,
                status = @Status, notes = @Notes,
                submitted_at = @SubmittedAt, submitted_by = @SubmittedBy,
                approved_at = @ApprovedAt, approved_by = @ApprovedBy,
                rejected_at = @RejectedAt, rejected_by = @RejectedBy, rejection_reason = @RejectionReason,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(v), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        const string sql = @"
            UPDATE valuations
            SET is_deleted = 1, deleted_at = @DeletedAt, updated_by = @DeletedBy, updated_at = @DeletedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0 AND status = 'draft'";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy?.ToString()
        }, _context.Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid clientId)
    {
        const string sql = "SELECT COUNT(1) FROM valuations WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";
        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);
        return count > 0;
    }

    // ──── Mapping ─────────────────────────────────────────────

    private static Valuation MapToValuation(dynamic r)
    {
        var v = (Valuation)Activator.CreateInstance(typeof(Valuation), nonPublic: true)!;
        // Use reflection to set private setters (same pattern as existing repositories)
        var type = typeof(Valuation);
        Set(v, "Id", Guid.Parse((string)r.id));
        Set(v, "ClientId", Guid.Parse((string)r.client_id));
        Set(v, "CompanyId", Guid.Parse((string)r.company_id));
        Set(v, "ValuationDate", (DateTime)r.valuation_date);
        Set(v, "EventType", (string)r.event_type);
        Set(v, "EventName", (string?)r.event_name);
        Set(v, "ValuationAmount", (decimal?)r.valuation_amount);
        Set(v, "TotalShares", (decimal)r.total_shares);
        Set(v, "PricePerShare", (decimal?)r.price_per_share);
        Set(v, "Status", (string)r.status);
        Set(v, "Notes", (string?)r.notes);
        Set(v, "SubmittedAt", (DateTime?)r.submitted_at);
        Set(v, "SubmittedBy", r.submitted_by is null ? (Guid?)null : Guid.Parse((string)r.submitted_by));
        Set(v, "ApprovedAt", (DateTime?)r.approved_at);
        Set(v, "ApprovedBy", r.approved_by is null ? (Guid?)null : Guid.Parse((string)r.approved_by));
        Set(v, "RejectedAt", (DateTime?)r.rejected_at);
        Set(v, "RejectedBy", r.rejected_by is null ? (Guid?)null : Guid.Parse((string)r.rejected_by));
        Set(v, "RejectionReason", (string?)r.rejection_reason);
        Set(v, "CreatedAt", (DateTime)r.created_at);
        Set(v, "UpdatedAt", (DateTime)r.updated_at);
        Set(v, "CreatedBy", r.created_by is null ? (Guid?)null : Guid.Parse((string)r.created_by));
        Set(v, "UpdatedBy", r.updated_by is null ? (Guid?)null : Guid.Parse((string)r.updated_by));
        Set(v, "IsDeleted", r.is_deleted == 1);
        return v;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(Valuation v) => new
    {
        Id = v.Id.ToString(),
        ClientId = v.ClientId.ToString(),
        CompanyId = v.CompanyId.ToString(),
        v.ValuationDate,
        v.EventType,
        v.EventName,
        v.ValuationAmount,
        v.TotalShares,
        v.PricePerShare,
        v.Status,
        v.Notes,
        v.SubmittedAt,
        SubmittedBy = v.SubmittedBy?.ToString(),
        v.ApprovedAt,
        ApprovedBy = v.ApprovedBy?.ToString(),
        v.RejectedAt,
        RejectedBy = v.RejectedBy?.ToString(),
        v.RejectionReason,
        v.CreatedAt,
        v.UpdatedAt,
        CreatedBy = v.CreatedBy?.ToString(),
        UpdatedBy = v.UpdatedBy?.ToString()
    };
}
