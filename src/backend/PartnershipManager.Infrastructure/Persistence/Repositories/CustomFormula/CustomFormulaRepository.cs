using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class CustomFormulaRepository : ICustomFormulaRepository
{
    private readonly DapperContext _context;

    public CustomFormulaRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ValuationCustomFormula> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        bool? isActive = null, string? sectorTag = null)
    {
        var where = "WHERE f.client_id = @ClientId AND f.company_id = @CompanyId AND f.is_deleted = 0";
        if (isActive.HasValue) where += " AND f.is_active = @IsActive";
        if (!string.IsNullOrWhiteSpace(sectorTag)) where += " AND f.sector_tag = @SectorTag";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                f.id, f.client_id, f.company_id, f.name, f.description, f.sector_tag,
                f.current_version_id, f.is_active,
                f.created_at, f.updated_at, f.created_by, f.updated_by, f.is_deleted, f.deleted_at
            FROM valuation_custom_formulas f
            {where}
            ORDER BY f.name ASC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            IsActive = isActive.HasValue ? (int?)(isActive.Value ? 1 : 0) : null,
            SectorTag = sectorTag,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToFormula), total);
    }

    public async Task<IEnumerable<ValuationCustomFormula>> GetActiveByCompanyAsync(Guid clientId, Guid companyId)
    {
        const string sql = @"
            SELECT f.id, f.client_id, f.company_id, f.name, f.description, f.sector_tag,
                f.current_version_id, f.is_active,
                f.created_at, f.updated_at, f.created_by, f.updated_by, f.is_deleted, f.deleted_at
            FROM valuation_custom_formulas f
            WHERE f.client_id = @ClientId AND f.company_id = @CompanyId
              AND f.is_active = 1 AND f.is_deleted = 0
            ORDER BY f.name ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToFormula);
    }

    public async Task<ValuationCustomFormula?> GetByIdAsync(Guid id, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, company_id, name, description, sector_tag,
                current_version_id, is_active,
                created_at, updated_at, created_by, updated_by, is_deleted, deleted_at
            FROM valuation_custom_formulas
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToFormula(row);
    }

    public async Task AddAsync(ValuationCustomFormula f)
    {
        const string sql = @"
            INSERT INTO valuation_custom_formulas (
                id, client_id, company_id, name, description, sector_tag,
                current_version_id, is_active,
                created_at, updated_at, created_by, updated_by, is_deleted
            ) VALUES (
                @Id, @ClientId, @CompanyId, @Name, @Description, @SectorTag,
                @CurrentVersionId, @IsActive,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, 0
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(f), _context.Transaction);
    }

    public async Task UpdateAsync(ValuationCustomFormula f)
    {
        const string sql = @"
            UPDATE valuation_custom_formulas SET
                name = @Name, description = @Description, sector_tag = @SectorTag,
                current_version_id = @CurrentVersionId, is_active = @IsActive,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(f), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        const string sql = @"
            UPDATE valuation_custom_formulas
            SET is_deleted = 1, deleted_at = @Now, updated_at = @Now, updated_by = @DeletedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            Now = DateTime.UtcNow,
            DeletedBy = deletedBy?.ToString()
        }, _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────

    private static Guid ParseGuid(object? val)
    {
        if (val is Guid g) return g;
        if (val is string s) return Guid.Parse(s);
        return Guid.Empty;
    }

    private static Guid? ParseNullableGuid(object? val)
        => val == null || val == DBNull.Value ? (Guid?)null : ParseGuid(val);

    private static bool ParseBool(object? val)
    {
        if (val is bool b) return b;
        if (val is sbyte sb) return sb != 0;
        if (val is int i) return i != 0;
        return Convert.ToBoolean(val);
    }

    private static ValuationCustomFormula MapToFormula(dynamic r)
    {
        var f = (ValuationCustomFormula)Activator.CreateInstance(typeof(ValuationCustomFormula), nonPublic: true)!;
        Set(f, "Id", ParseGuid(r.id));
        Set(f, "ClientId", ParseGuid(r.client_id));
        Set(f, "CompanyId", ParseGuid(r.company_id));
        Set(f, "Name", (string)r.name);
        Set(f, "Description", (string?)r.description);
        Set(f, "SectorTag", (string?)r.sector_tag);
        Set(f, "CurrentVersionId", ParseNullableGuid(r.current_version_id));
        Set(f, "IsActive", ParseBool(r.is_active));
        Set(f, "CreatedAt", (DateTime)r.created_at);
        Set(f, "UpdatedAt", (DateTime)r.updated_at);
        Set(f, "CreatedBy", ParseNullableGuid(r.created_by));
        Set(f, "UpdatedBy", ParseNullableGuid(r.updated_by));
        Set(f, "IsDeleted", ParseBool(r.is_deleted));
        return f;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(ValuationCustomFormula f) => new
    {
        Id = f.Id.ToString(),
        ClientId = f.ClientId.ToString(),
        CompanyId = f.CompanyId.ToString(),
        f.Name,
        f.Description,
        f.SectorTag,
        CurrentVersionId = f.CurrentVersionId?.ToString(),
        IsActive = f.IsActive ? 1 : 0,
        f.CreatedAt,
        f.UpdatedAt,
        CreatedBy = f.CreatedBy?.ToString(),
        UpdatedBy = f.UpdatedBy?.ToString()
    };
}
