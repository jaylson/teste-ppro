using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>FormulaVersionRepository — immutable versions, no update method.</summary>
public class FormulaVersionRepository : IFormulaVersionRepository
{
    private readonly DapperContext _context;

    public FormulaVersionRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        v.id, v.formula_id, v.client_id, v.version_number,
        v.expression, v.variables, v.result_unit, v.result_label,
        v.test_inputs, v.test_result,
        v.validation_status, v.validation_errors,
        v.created_at, v.created_by";

    public async Task<IEnumerable<ValuationFormulaVersion>> GetByFormulaAsync(Guid formulaId, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM valuation_formula_versions v
            WHERE v.formula_id = @FormulaId AND v.client_id = @ClientId
            ORDER BY v.version_number DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            FormulaId = formulaId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVersion);
    }

    public async Task<ValuationFormulaVersion?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM valuation_formula_versions v
            WHERE v.id = @Id AND v.client_id = @ClientId";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToVersion(row);
    }

    public async Task<ValuationFormulaVersion?> GetCurrentVersionAsync(Guid formulaId, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM valuation_formula_versions v
            INNER JOIN valuation_custom_formulas f ON f.current_version_id = v.id
            WHERE f.id = @FormulaId AND v.client_id = @ClientId";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            FormulaId = formulaId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToVersion(row);
    }

    public async Task<int> GetNextVersionNumberAsync(Guid formulaId)
    {
        const string sql = @"
            SELECT COALESCE(MAX(version_number), 0) + 1
            FROM valuation_formula_versions
            WHERE formula_id = @FormulaId";

        return await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            FormulaId = formulaId.ToString()
        }, _context.Transaction);
    }

    public async Task AddAsync(ValuationFormulaVersion v)
    {
        const string sql = @"
            INSERT INTO valuation_formula_versions (
                id, formula_id, client_id, version_number,
                expression, variables, result_unit, result_label,
                test_inputs, test_result,
                validation_status, validation_errors,
                created_at, created_by
            ) VALUES (
                @Id, @FormulaId, @ClientId, @VersionNumber,
                @Expression, @Variables, @ResultUnit, @ResultLabel,
                @TestInputs, @TestResult,
                @ValidationStatus, @ValidationErrors,
                @CreatedAt, @CreatedBy
            )";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = v.Id.ToString(),
            FormulaId = v.FormulaId.ToString(),
            ClientId = v.ClientId.ToString(),
            v.VersionNumber,
            v.Expression,
            v.Variables,
            v.ResultUnit,
            v.ResultLabel,
            v.TestInputs,
            v.TestResult,
            v.ValidationStatus,
            v.ValidationErrors,
            v.CreatedAt,
            CreatedBy = v.CreatedBy.ToString()
        }, _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────

    private static Guid ParseGuid(dynamic val)
        => val is Guid g ? g : Guid.Parse((string)val);

    private static ValuationFormulaVersion MapToVersion(dynamic r)
    {
        var v = (ValuationFormulaVersion)Activator.CreateInstance(typeof(ValuationFormulaVersion), nonPublic: true)!;
        Set(v, "Id", ParseGuid(r.id));
        Set(v, "FormulaId", ParseGuid(r.formula_id));
        Set(v, "ClientId", ParseGuid(r.client_id));
        Set(v, "VersionNumber", (int)r.version_number);
        Set(v, "Expression", (string)r.expression);
        Set(v, "Variables", (string)r.variables);
        Set(v, "ResultUnit", (string)r.result_unit);
        Set(v, "ResultLabel", (string?)r.result_label);
        Set(v, "TestInputs", (string?)r.test_inputs);
        Set(v, "TestResult", (decimal?)r.test_result);
        Set(v, "ValidationStatus", (string)r.validation_status);
        Set(v, "ValidationErrors", (string?)r.validation_errors);
        Set(v, "CreatedAt", (DateTime)r.created_at);
        Set(v, "CreatedBy", ParseGuid(r.created_by));
        return v;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }
}

/// <summary>FormulaExecutionRepository — immutable audit log, no update or delete.</summary>
public class FormulaExecutionRepository : IFormulaExecutionRepository
{
    private readonly DapperContext _context;

    public FormulaExecutionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ValuationFormulaExecution>> GetByMethodAsync(
        Guid valuationMethodId, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, valuation_method_id, formula_version_id,
                inputs_used, calculated_value, expression_snapshot,
                executed_by, executed_at
            FROM valuation_formula_executions
            WHERE valuation_method_id = @MethodId AND client_id = @ClientId
            ORDER BY executed_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            MethodId = valuationMethodId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToExecution);
    }

    public async Task<IEnumerable<ValuationFormulaExecution>> GetByVersionAsync(
        Guid formulaVersionId, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id, valuation_method_id, formula_version_id,
                inputs_used, calculated_value, expression_snapshot,
                executed_by, executed_at
            FROM valuation_formula_executions
            WHERE formula_version_id = @VersionId AND client_id = @ClientId
            ORDER BY executed_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            VersionId = formulaVersionId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToExecution);
    }

    public async Task AddAsync(ValuationFormulaExecution e)
    {
        const string sql = @"
            INSERT INTO valuation_formula_executions (
                id, client_id, valuation_method_id, formula_version_id,
                inputs_used, calculated_value, expression_snapshot,
                executed_by, executed_at
            ) VALUES (
                @Id, @ClientId, @ValuationMethodId, @FormulaVersionId,
                @InputsUsed, @CalculatedValue, @ExpressionSnapshot,
                @ExecutedBy, @ExecutedAt
            )";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = e.Id.ToString(),
            ClientId = e.ClientId.ToString(),
            ValuationMethodId = e.ValuationMethodId.ToString(),
            FormulaVersionId = e.FormulaVersionId.ToString(),
            e.InputsUsed,
            e.CalculatedValue,
            e.ExpressionSnapshot,
            ExecutedBy = e.ExecutedBy.ToString(),
            e.ExecutedAt
        }, _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────

    private static Guid ParseGuid(dynamic val)
        => val is Guid g ? g : Guid.Parse((string)val);

    private static ValuationFormulaExecution MapToExecution(dynamic r)
    {
        var e = (ValuationFormulaExecution)Activator.CreateInstance(typeof(ValuationFormulaExecution), nonPublic: true)!;
        Set(e, "Id", ParseGuid(r.id));
        Set(e, "ClientId", ParseGuid(r.client_id));
        Set(e, "ValuationMethodId", ParseGuid(r.valuation_method_id));
        Set(e, "FormulaVersionId", ParseGuid(r.formula_version_id));
        Set(e, "InputsUsed", (string)r.inputs_used);
        Set(e, "CalculatedValue", (decimal)r.calculated_value);
        Set(e, "ExpressionSnapshot", (string)r.expression_snapshot);
        Set(e, "ExecutedBy", ParseGuid(r.executed_by));
        Set(e, "ExecutedAt", (DateTime)r.executed_at);
        return e;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }
}
