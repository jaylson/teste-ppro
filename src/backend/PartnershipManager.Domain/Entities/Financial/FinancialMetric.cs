namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Financial KPI metrics for a monthly period.
/// ARR and RunwayMonths are calculated server-side when metrics are saved.
/// One row per FinancialPeriod (UNIQUE on period_id).
/// </summary>
public class FinancialMetric : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid PeriodId { get; private set; }

    // Revenue
    public decimal? GrossRevenue { get; private set; }
    public decimal? NetRevenue { get; private set; }
    public decimal? Mrr { get; private set; }
    /// <summary>Calculated: ARR = MRR × 12</summary>
    public decimal? Arr { get; private set; }

    // Cash & Burn
    public decimal? CashBalance { get; private set; }
    public decimal? BurnRate { get; private set; }
    /// <summary>Calculated: Runway = CashBalance / BurnRate (months)</summary>
    public decimal? RunwayMonths { get; private set; }

    // Unit Economics
    public int? CustomerCount { get; private set; }
    public decimal? ChurnRate { get; private set; }
    public decimal? Cac { get; private set; }
    public decimal? Ltv { get; private set; }
    public short? Nps { get; private set; }

    // Profitability
    public decimal? Ebitda { get; private set; }
    /// <summary>Calculated: EbitdaMargin = EBITDA / NetRevenue × 100</summary>
    public decimal? EbitdaMargin { get; private set; }
    public decimal? NetIncome { get; private set; }

    private FinancialMetric() { }

    public static FinancialMetric Create(
        Guid clientId,
        Guid periodId,
        Guid? createdBy = null)
    {
        var fm = new FinancialMetric
        {
            ClientId = clientId,
            PeriodId = periodId
        };
        fm.CreatedBy = createdBy;
        fm.UpdatedBy = createdBy;
        return fm;
    }

    public void UpdateRevenue(
        decimal? grossRevenue,
        decimal? netRevenue,
        decimal? mrr,
        Guid updatedBy)
    {
        ValidateNonNegative(grossRevenue, nameof(grossRevenue));
        ValidateNonNegative(netRevenue, nameof(netRevenue));
        ValidateNonNegative(mrr, nameof(mrr));

        GrossRevenue = grossRevenue;
        NetRevenue = netRevenue;
        Mrr = mrr;
        // Auto-calculate ARR
        Arr = mrr.HasValue ? Math.Round(mrr.Value * 12, 2) : null;
        // Recalculate EBITDA margin if EBITDA is set
        RecalculateEbitdaMargin();

        MarkUpdated(updatedBy);
    }

    public void UpdateCashBurn(
        decimal? cashBalance,
        decimal? burnRate,
        Guid updatedBy)
    {
        ValidateNonNegative(cashBalance, nameof(cashBalance));
        ValidateNonNegative(burnRate, nameof(burnRate));

        CashBalance = cashBalance;
        BurnRate = burnRate;
        // Auto-calculate Runway
        RunwayMonths = (cashBalance.HasValue && burnRate.HasValue && burnRate > 0)
            ? Math.Round(cashBalance.Value / burnRate.Value, 2)
            : null;

        MarkUpdated(updatedBy);
    }

    public void UpdateUnitEconomics(
        int? customerCount,
        decimal? churnRate,
        decimal? cac,
        decimal? ltv,
        short? nps,
        Guid updatedBy)
    {
        if (customerCount.HasValue && customerCount < 0)
            throw new ArgumentOutOfRangeException(nameof(customerCount), "Número de clientes não pode ser negativo.");
        if (churnRate.HasValue && (churnRate < 0 || churnRate > 100))
            throw new ArgumentOutOfRangeException(nameof(churnRate), "Churn rate deve estar entre 0 e 100.");
        if (nps.HasValue && (nps < -100 || nps > 100))
            throw new ArgumentOutOfRangeException(nameof(nps), "NPS deve estar entre -100 e 100.");

        CustomerCount = customerCount;
        ChurnRate = churnRate;
        Cac = cac;
        Ltv = ltv;
        Nps = nps;
        MarkUpdated(updatedBy);
    }

    public void UpdateProfitability(
        decimal? ebitda,
        decimal? netIncome,
        Guid updatedBy)
    {
        Ebitda = ebitda;
        NetIncome = netIncome;
        RecalculateEbitdaMargin();
        MarkUpdated(updatedBy);
    }

    // ──── Derived calculations ────────────────────────────────

    /// <summary>
    /// Returns runway status for UI color coding:
    /// Green (> 12 months), Yellow (6–12), Red (< 6)
    /// </summary>
    public string? RunwayStatus => RunwayMonths switch
    {
        null => null,
        >= 12 => "green",
        >= 6 => "yellow",
        _ => "red"
    };

    public decimal? LtvToCacRatio => (Ltv.HasValue && Cac.HasValue && Cac > 0)
        ? Math.Round(Ltv.Value / Cac.Value, 2)
        : null;

    // ──── Private helpers ─────────────────────────────────────

    private void RecalculateEbitdaMargin()
    {
        EbitdaMargin = (Ebitda.HasValue && NetRevenue.HasValue && NetRevenue > 0)
            ? Math.Round(Ebitda.Value / NetRevenue.Value * 100, 4)
            : null;
    }

    private static void ValidateNonNegative(decimal? value, string paramName)
    {
        if (value.HasValue && value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} não pode ser negativo.");
    }

    private void MarkUpdated(Guid updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
