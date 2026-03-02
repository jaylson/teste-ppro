using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class ValuationMethodRepository : IValuationMethodRepository
{
    private readonly DapperContext _context;

    public ValuationMethodRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ValuationMethod>> GetByValuationAsync(Guid valuationId, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, valuation_id, method_type, is_selected,
                calculated_value, inputs, data_source, notes, formula_version_id,
                created_at, updated_at, created_by, updated_by
            FROM valuation_methods
            WHERE valuation_id = @ValuationId AND client_id = @ClientId
            ORDER BY is_selected DESC, created_at ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ValuationId = valuationId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToMethod);
    }

    public async Task<ValuationMethod?> GetByIdAsync(Guid id, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, valuation_id, method_type, is_selected,
                calculated_value, inputs, data_source, notes, formula_version_id,
                created_at, updated_at, created_by, updated_by
            FROM valuation_methods
            WHERE id = @Id AND client_id = @ClientId";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToMethod(row);
    }

    public async Task<ValuationMethod?> GetSelectedAsync(Guid valuationId, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, valuation_id, method_type, is_selected,
                calculated_value, inputs, data_source, notes, formula_version_id,
                created_at, updated_at, created_by, updated_by
            FROM valuation_methods
            WHERE valuation_id = @ValuationId AND client_id = @ClientId AND is_selected = 1
            LIMIT 1";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ValuationId = valuationId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToMethod(row);
    }

    public async Task AddAsync(ValuationMethod m)
    {
        const string sql = @"
            INSERT INTO valuation_methods (
                id, client_id, valuation_id, method_type, is_selected,
                calculated_value, inputs, data_source, notes, formula_version_id,
                created_at, updated_at, created_by, updated_by
            ) VALUES (
                @Id, @ClientId, @ValuationId, @MethodType, @IsSelected,
                @CalculatedValue, @Inputs, @DataSource, @Notes, @FormulaVersionId,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(m), _context.Transaction);
    }

    public async Task UpdateAsync(ValuationMethod m)
    {
        const string sql = @"
            UPDATE valuation_methods SET
                method_type = @MethodType, is_selected = @IsSelected,
                calculated_value = @CalculatedValue, inputs = @Inputs,
                data_source = @DataSource, notes = @Notes,
                formula_version_id = @FormulaVersionId,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId";

        await _context.Connection.ExecuteAsync(sql, ToParams(m), _context.Transaction);
    }

    public async Task DeleteAsync(Guid id, Guid clientId)
    {
        const string sql = "DELETE FROM valuation_methods WHERE id = @Id AND client_id = @ClientId";
        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);
    }

    /// <summary>
    /// Atomically deselects all methods for the valuation, then selects the specified one.
    /// Enforces VA-01: exactly one selected per valuation.
    /// </summary>
    public async Task SetSelectedAsync(Guid valuationId, Guid methodId, Guid clientId, Guid updatedBy)
    {
        const string deselect = @"
            UPDATE valuation_methods
            SET is_selected = 0, updated_at = @Now, updated_by = @UpdatedBy
            WHERE valuation_id = @ValuationId AND client_id = @ClientId";

        const string select = @"
            UPDATE valuation_methods
            SET is_selected = 1, updated_at = @Now, updated_by = @UpdatedBy
            WHERE id = @MethodId AND valuation_id = @ValuationId AND client_id = @ClientId";

        var p = new
        {
            ValuationId = valuationId.ToString(),
            MethodId = methodId.ToString(),
            ClientId = clientId.ToString(),
            UpdatedBy = updatedBy.ToString(),
            Now = DateTime.UtcNow
        };

        await _context.Connection.ExecuteAsync(deselect, p, _context.Transaction);
        await _context.Connection.ExecuteAsync(select, p, _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────
    private static ValuationMethod MapToMethod(dynamic r)
    {
        var m = (ValuationMethod)Activator.CreateInstance(typeof(ValuationMethod), nonPublic: true)!;
        Set(m, "Id", Guid.Parse((string)r.id));
        Set(m, "ClientId", Guid.Parse((string)r.client_id));
        Set(m, "ValuationId", Guid.Parse((string)r.valuation_id));
        Set(m, "MethodType", (string)r.method_type);
        Set(m, "IsSelected", ((sbyte?)r.is_selected ?? 0) == 1);
        Set(m, "CalculatedValue", (decimal?)r.calculated_value);
        Set(m, "Inputs", (string?)r.inputs);
        Set(m, "DataSource", (string?)r.data_source);
        Set(m, "Notes", (string?)r.notes);
        Set(m, "FormulaVersionId", r.formula_version_id is null ? (Guid?)null : Guid.Parse((string)r.formula_version_id));
        Set(m, "CreatedAt", (DateTime)r.created_at);
        Set(m, "UpdatedAt", (DateTime)r.updated_at);
        Set(m, "CreatedBy", r.created_by is null ? (Guid?)null : Guid.Parse((string)r.created_by));
        Set(m, "UpdatedBy", r.updated_by is null ? (Guid?)null : Guid.Parse((string)r.updated_by));
        return m;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(ValuationMethod m) => new
    {
        Id = m.Id.ToString(),
        ClientId = m.ClientId.ToString(),
        ValuationId = m.ValuationId.ToString(),
        m.MethodType,
        IsSelected = m.IsSelected ? 1 : 0,
        m.CalculatedValue,
        m.Inputs,
        m.DataSource,
        m.Notes,
        FormulaVersionId = m.FormulaVersionId?.ToString(),
        m.CreatedAt,
        m.UpdatedAt,
        CreatedBy = m.CreatedBy?.ToString(),
        UpdatedBy = m.UpdatedBy?.ToString()
    };
}
