using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class FinancialPeriodRepository : IFinancialPeriodRepository
{
    private readonly DapperContext _context;

    public FinancialPeriodRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<FinancialPeriod> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        int? year = null, string? status = null)
    {
        var where = "WHERE fp.client_id = @ClientId AND fp.company_id = @CompanyId AND fp.is_deleted = 0";
        if (year.HasValue) where += " AND fp.year = @Year";
        if (!string.IsNullOrWhiteSpace(status)) where += " AND fp.status = @Status";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                fp.id, fp.client_id, fp.company_id, fp.year, fp.month, fp.status, fp.notes,
                fp.submitted_at, fp.submitted_by, fp.approved_at, fp.approved_by,
                fp.locked_at, fp.locked_by,
                fp.created_at, fp.updated_at, fp.created_by, fp.updated_by,
                fp.is_deleted, fp.deleted_at
            FROM financial_periods fp
            {where}
            ORDER BY fp.year DESC, fp.month DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Year = year,
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToPeriod), total);
    }

    public async Task<IEnumerable<FinancialPeriod>> GetByYearAsync(Guid clientId, Guid companyId, short year)
    {
        const string sql = @"
            SELECT id, client_id, company_id, year, month, status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                locked_at, locked_by,
                created_at, updated_at, created_by, updated_by, is_deleted, deleted_at
            FROM financial_periods
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND year = @Year AND is_deleted = 0
            ORDER BY month ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Year = year
        }, _context.Transaction);

        return rows.Select(MapToPeriod);
    }

    public async Task<FinancialPeriod?> GetByIdAsync(Guid id, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, company_id, year, month, status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                locked_at, locked_by,
                created_at, updated_at, created_by, updated_by, is_deleted, deleted_at
            FROM financial_periods
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToPeriod(row);
    }

    public async Task<FinancialPeriod?> GetByYearMonthAsync(Guid clientId, Guid companyId, short year, byte month)
    {
        const string sql = @"
            SELECT id, client_id, company_id, year, month, status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                locked_at, locked_by,
                created_at, updated_at, created_by, updated_by, is_deleted, deleted_at
            FROM financial_periods
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND year = @Year AND month = @Month AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Year = year,
            Month = month
        }, _context.Transaction);

        return row is null ? null : MapToPeriod(row);
    }

    public async Task<FinancialPeriod?> GetPreviousPeriodAsync(Guid clientId, Guid companyId, short year, byte month)
    {
        // Calculate previous month/year
        short prevYear = month == 1 ? (short)(year - 1) : year;
        byte prevMonth = month == 1 ? (byte)12 : (byte)(month - 1);
        return await GetByYearMonthAsync(clientId, companyId, prevYear, prevMonth);
    }

    public async Task AddAsync(FinancialPeriod fp)
    {
        const string sql = @"
            INSERT INTO financial_periods (
                id, client_id, company_id, year, month, status, notes,
                submitted_at, submitted_by, approved_at, approved_by,
                locked_at, locked_by,
                created_at, updated_at, created_by, updated_by, is_deleted
            ) VALUES (
                @Id, @ClientId, @CompanyId, @Year, @Month, @Status, @Notes,
                @SubmittedAt, @SubmittedBy, @ApprovedAt, @ApprovedBy,
                @LockedAt, @LockedBy,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, 0
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(fp), _context.Transaction);
    }

    public async Task UpdateAsync(FinancialPeriod fp)
    {
        const string sql = @"
            UPDATE financial_periods SET
                status = @Status, notes = @Notes,
                submitted_at = @SubmittedAt, submitted_by = @SubmittedBy,
                approved_at = @ApprovedAt, approved_by = @ApprovedBy,
                locked_at = @LockedAt, locked_by = @LockedBy,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(fp), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        const string sql = @"
            UPDATE financial_periods
            SET is_deleted = 1, deleted_at = @Now, updated_at = @Now, updated_by = @DeletedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0 AND status = 'draft'";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            Now = DateTime.UtcNow,
            DeletedBy = deletedBy?.ToString()
        }, _context.Transaction);
    }

    public async Task<bool> ExistsAsync(Guid clientId, Guid companyId, short year, byte month)
    {
        const string sql = @"
            SELECT COUNT(1) FROM financial_periods
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND year = @Year AND month = @Month AND is_deleted = 0";

        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Year = year,
            Month = month
        }, _context.Transaction);
        return count > 0;
    }

    // ──── Mapping ─────────────────────────────────
    private static FinancialPeriod MapToPeriod(dynamic r)
    {
        var fp = (FinancialPeriod)Activator.CreateInstance(typeof(FinancialPeriod), nonPublic: true)!;
        Set(fp, "Id", Guid.Parse((string)r.id));
        Set(fp, "ClientId", Guid.Parse((string)r.client_id));
        Set(fp, "CompanyId", Guid.Parse((string)r.company_id));
        Set(fp, "Year", (short)r.year);
        Set(fp, "Month", (byte)r.month);
        Set(fp, "Status", (string)r.status);
        Set(fp, "Notes", (string?)r.notes);
        Set(fp, "SubmittedAt", (DateTime?)r.submitted_at);
        Set(fp, "SubmittedBy", r.submitted_by is null ? (Guid?)null : Guid.Parse((string)r.submitted_by));
        Set(fp, "ApprovedAt", (DateTime?)r.approved_at);
        Set(fp, "ApprovedBy", r.approved_by is null ? (Guid?)null : Guid.Parse((string)r.approved_by));
        Set(fp, "LockedAt", (DateTime?)r.locked_at);
        Set(fp, "LockedBy", r.locked_by is null ? (Guid?)null : Guid.Parse((string)r.locked_by));
        Set(fp, "CreatedAt", (DateTime)r.created_at);
        Set(fp, "UpdatedAt", (DateTime)r.updated_at);
        Set(fp, "CreatedBy", r.created_by is null ? (Guid?)null : Guid.Parse((string)r.created_by));
        Set(fp, "UpdatedBy", r.updated_by is null ? (Guid?)null : Guid.Parse((string)r.updated_by));
        Set(fp, "IsDeleted", r.is_deleted == 1);
        return fp;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(FinancialPeriod fp) => new
    {
        Id = fp.Id.ToString(),
        ClientId = fp.ClientId.ToString(),
        CompanyId = fp.CompanyId.ToString(),
        fp.Year,
        fp.Month,
        fp.Status,
        fp.Notes,
        fp.SubmittedAt,
        SubmittedBy = fp.SubmittedBy?.ToString(),
        fp.ApprovedAt,
        ApprovedBy = fp.ApprovedBy?.ToString(),
        fp.LockedAt,
        LockedBy = fp.LockedBy?.ToString(),
        fp.CreatedAt,
        fp.UpdatedAt,
        CreatedBy = fp.CreatedBy?.ToString(),
        UpdatedBy = fp.UpdatedBy?.ToString()
    };
}
