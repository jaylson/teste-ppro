using PartnershipManager.Application.Features.Valuation.DTOs;
using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Infrastructure.Services.Valuation;

/// <summary>
/// Pure computation engine for all 8 valuation methodologies.
/// Stateless — no DB calls, no IUnitOfWork. All calculations in-memory.
/// </summary>
public interface IValuationCalculationEngine
{
    /// <summary>
    /// Calculate valuation using the given methodology and inputs.
    /// Returns the calculated value plus a breakdown dictionary for transparency.
    /// </summary>
    Task<CalculateMethodResponse> CalculateAsync(
        string methodType,
        Dictionary<string, decimal> inputs,
        string? formulaExpression = null,
        IEnumerable<FormulaVariableDefinition>? formulaVariables = null);

    /// <summary>Validate that all required inputs exist for the given method type.</summary>
    IReadOnlyList<string> ValidateInputs(string methodType, Dictionary<string, decimal> inputs);
}

public class ValuationCalculationEngine : IValuationCalculationEngine
{
    private readonly ICustomFormulaEngine _customFormulaEngine;

    public ValuationCalculationEngine(ICustomFormulaEngine customFormulaEngine)
    {
        _customFormulaEngine = customFormulaEngine;
    }

    public Task<CalculateMethodResponse> CalculateAsync(
        string methodType,
        Dictionary<string, decimal> inputs,
        string? formulaExpression = null,
        IEnumerable<FormulaVariableDefinition>? formulaVariables = null)
    {
        var errors = ValidateInputs(methodType, inputs);
        if (errors.Count > 0)
            throw new ArgumentException($"Inputs inválidos para '{methodType}': {string.Join("; ", errors)}");

        CalculateMethodResponse result = methodType switch
        {
            ValuationMethodTypes.ArrMultiple => CalculateArrMultiple(inputs),
            ValuationMethodTypes.Dcf => CalculateDcf(inputs),
            ValuationMethodTypes.Comparables => CalculateComparables(inputs),
            ValuationMethodTypes.EbitdaMultiple => CalculateEbitdaMultiple(inputs),
            ValuationMethodTypes.MrrMultiple => CalculateMrrMultiple(inputs),
            ValuationMethodTypes.AssetBased => CalculateAssetBased(inputs),
            ValuationMethodTypes.Berkus => CalculateBerkus(inputs),
            ValuationMethodTypes.Custom => CalculateCustom(inputs, formulaExpression!, formulaVariables),
            _ => throw new NotSupportedException($"Metodologia '{methodType}' não suportada.")
        };

        return Task.FromResult(result);
    }

    public IReadOnlyList<string> ValidateInputs(string methodType, Dictionary<string, decimal> inputs)
    {
        var errors = new List<string>();

        var required = GetRequiredInputs(methodType);
        foreach (var key in required)
        {
            if (!inputs.ContainsKey(key))
                errors.Add($"Input '{key}' é obrigatório para '{methodType}'.");
        }

        // Validate non-negative values for financial figures
        foreach (var (k, v) in inputs)
        {
            if (v < 0 && !AllowNegativeInputs.Contains(k))
                errors.Add($"Input '{k}' não pode ser negativo.");
        }

        return errors;
    }

    // ─── ARR Multiple ─────────────────────────────────────────────────────────
    // Formula: Valuation = ARR × Multiple
    private static CalculateMethodResponse CalculateArrMultiple(Dictionary<string, decimal> inputs)
    {
        var arr = inputs["arr"];
        var multiple = inputs["multiple"];

        if (multiple <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputs), "Multiple deve ser positivo.");

        var value = Math.Round(arr * multiple, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.ArrMultiple,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["arr"] = arr,
                ["multiple"] = multiple,
                ["formula"] = "ARR × Multiple",
                ["result"] = value
            }
        };
    }

    // ─── DCF (Discounted Cash Flow) ───────────────────────────────────────────
    // Inputs: annual_cash_flow, growth_rate (%), discount_rate (%), terminal_multiple, projection_years
    // Formula: Sum of PV of cash flows + Terminal Value
    private static CalculateMethodResponse CalculateDcf(Dictionary<string, decimal> inputs)
    {
        var annualCashFlow = inputs["annual_cash_flow"];
        var growthRate = inputs["growth_rate"] / 100m;
        var discountRate = inputs["discount_rate"] / 100m;
        var terminalMultiple = inputs.GetValueOrDefault("terminal_multiple", 10m);
        var projectionYears = (int)inputs.GetValueOrDefault("projection_years", 5m);

        if (discountRate <= 0)
            throw new ArgumentException("Discount rate deve ser > 0.");
        if (projectionYears < 1 || projectionYears > 20)
            throw new ArgumentOutOfRangeException(nameof(inputs), "Projection years deve estar entre 1 e 20.");

        decimal pvSum = 0;
        var yearlyBreakdown = new Dictionary<int, decimal>();

        for (int year = 1; year <= projectionYears; year++)
        {
            var cashFlow = annualCashFlow * (decimal)Math.Pow((double)(1 + growthRate), year);
            var pv = cashFlow / (decimal)Math.Pow((double)(1 + discountRate), year);
            pvSum += pv;
            yearlyBreakdown[year] = Math.Round(pv, 2);
        }

        // Terminal Value using exit multiple on final year cash flow
        var finalYearCf = annualCashFlow * (decimal)Math.Pow((double)(1 + growthRate), projectionYears);
        var terminalValue = finalYearCf * terminalMultiple;
        var pvTerminal = terminalValue / (decimal)Math.Pow((double)(1 + discountRate), projectionYears);

        var totalValue = Math.Round(pvSum + pvTerminal, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.Dcf,
            CalculatedValue = totalValue,
            Breakdown = new Dictionary<string, object>
            {
                ["annual_cash_flow"] = annualCashFlow,
                ["growth_rate_pct"] = inputs["growth_rate"],
                ["discount_rate_pct"] = inputs["discount_rate"],
                ["terminal_multiple"] = terminalMultiple,
                ["projection_years"] = projectionYears,
                ["pv_cash_flows"] = Math.Round(pvSum, 2),
                ["pv_terminal_value"] = Math.Round(pvTerminal, 2),
                ["yearly_pvs"] = yearlyBreakdown,
                ["formula"] = "Σ PV(Cash Flows) + PV(Terminal Value)"
            }
        };
    }

    // ─── Comparables / Revenue Multiple ──────────────────────────────────────
    // Formula: Valuation = Revenue × Industry Multiple
    private static CalculateMethodResponse CalculateComparables(Dictionary<string, decimal> inputs)
    {
        var revenue = inputs["revenue"];
        var multiple = inputs["multiple"];

        if (multiple <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputs), "Multiple deve ser positivo.");

        var value = Math.Round(revenue * multiple, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.Comparables,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["revenue"] = revenue,
                ["multiple"] = multiple,
                ["formula"] = "Revenue × Industry Multiple"
            }
        };
    }

    // ─── EBITDA Multiple ──────────────────────────────────────────────────────
    // Formula: Valuation = EBITDA × EV/EBITDA Multiple
    private static CalculateMethodResponse CalculateEbitdaMultiple(Dictionary<string, decimal> inputs)
    {
        var ebitda = inputs["ebitda"];
        var multiple = inputs["multiple"];

        if (multiple <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputs), "Multiple deve ser positivo.");

        var value = Math.Round(ebitda * multiple, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.EbitdaMultiple,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["ebitda"] = ebitda,
                ["multiple"] = multiple,
                ["formula"] = "EBITDA × EV/EBITDA Multiple"
            }
        };
    }

    // ─── MRR Multiple ─────────────────────────────────────────────────────────
    // Formula: Valuation = MRR × Multiple
    private static CalculateMethodResponse CalculateMrrMultiple(Dictionary<string, decimal> inputs)
    {
        var mrr = inputs["mrr"];
        var multiple = inputs["multiple"];

        if (multiple <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputs), "Multiple deve ser positivo.");

        var arr = mrr * 12;
        var value = Math.Round(mrr * multiple, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.MrrMultiple,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["mrr"] = mrr,
                ["arr"] = Math.Round(arr, 2),
                ["multiple"] = multiple,
                ["formula"] = "MRR × Multiple"
            }
        };
    }

    // ─── Asset-Based ──────────────────────────────────────────────────────────
    // Formula: NAV = Total Assets − Total Liabilities
    private static CalculateMethodResponse CalculateAssetBased(Dictionary<string, decimal> inputs)
    {
        var totalAssets = inputs["total_assets"];
        var totalLiabilities = inputs["total_liabilities"];
        var adjustmentFactor = inputs.GetValueOrDefault("adjustment_factor", 1m);

        var nav = (totalAssets - totalLiabilities) * adjustmentFactor;
        var value = Math.Round(nav, 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.AssetBased,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["total_assets"] = totalAssets,
                ["total_liabilities"] = totalLiabilities,
                ["adjustment_factor"] = adjustmentFactor,
                ["net_asset_value"] = value,
                ["formula"] = "(Total Assets - Total Liabilities) × Adjustment Factor"
            }
        };
    }

    // ─── Berkus Method (pre-revenue startups) ────────────────────────────────
    // 5 components, max 500k each, max 2.5M total
    // Inputs: sound_idea, prototype, management_team, strategic_relationships, product_rollout
    // Each value 0–500000
    private static CalculateMethodResponse CalculateBerkus(Dictionary<string, decimal> inputs)
    {
        var components = new Dictionary<string, decimal>
        {
            ["sound_idea"] = Math.Min(inputs.GetValueOrDefault("sound_idea", 0), 500_000),
            ["prototype"] = Math.Min(inputs.GetValueOrDefault("prototype", 0), 500_000),
            ["management_team"] = Math.Min(inputs.GetValueOrDefault("management_team", 0), 500_000),
            ["strategic_relationships"] = Math.Min(inputs.GetValueOrDefault("strategic_relationships", 0), 500_000),
            ["product_rollout"] = Math.Min(inputs.GetValueOrDefault("product_rollout", 0), 500_000),
        };

        var value = Math.Round(components.Values.Sum(), 2);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.Berkus,
            CalculatedValue = value,
            Breakdown = new Dictionary<string, object>
            {
                ["sound_idea"] = components["sound_idea"],
                ["prototype"] = components["prototype"],
                ["management_team"] = components["management_team"],
                ["strategic_relationships"] = components["strategic_relationships"],
                ["product_rollout"] = components["product_rollout"],
                ["max_possible"] = 2_500_000m,
                ["formula"] = "Sum of 5 Berkus components (max 500K each)"
            }
        };
    }

    // ─── Custom Formula ───────────────────────────────────────────────────────
    private CalculateMethodResponse CalculateCustom(
        Dictionary<string, decimal> inputs,
        string formulaExpression,
        IEnumerable<FormulaVariableDefinition>? variables)
    {
        if (string.IsNullOrWhiteSpace(formulaExpression))
            throw new ArgumentException("Expressão da fórmula é obrigatória para metodologia 'custom'.");

        var result = _customFormulaEngine.Evaluate(formulaExpression, inputs);

        return new CalculateMethodResponse
        {
            MethodType = ValuationMethodTypes.Custom,
            CalculatedValue = result,
            FormulaExpression = formulaExpression,
            Breakdown = inputs.ToDictionary(kv => kv.Key, kv => (object)kv.Value)
        };
    }

    // ─── Input requirements ───────────────────────────────────────────────────

    private static IReadOnlyList<string> GetRequiredInputs(string methodType) => methodType switch
    {
        ValuationMethodTypes.ArrMultiple => ["arr", "multiple"],
        ValuationMethodTypes.Dcf => ["annual_cash_flow", "growth_rate", "discount_rate"],
        ValuationMethodTypes.Comparables => ["revenue", "multiple"],
        ValuationMethodTypes.EbitdaMultiple => ["ebitda", "multiple"],
        ValuationMethodTypes.MrrMultiple => ["mrr", "multiple"],
        ValuationMethodTypes.AssetBased => ["total_assets", "total_liabilities"],
        ValuationMethodTypes.Berkus => [],  // all optional (all default to 0)
        ValuationMethodTypes.Custom => [],  // validated by FormulaSecurityValidator
        _ => []
    };

    private static readonly HashSet<string> AllowNegativeInputs = ["ebitda", "net_income", "annual_cash_flow"];
}
