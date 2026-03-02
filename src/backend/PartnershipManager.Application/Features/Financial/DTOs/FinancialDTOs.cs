using PartnershipManager.Application.Common.Models;

namespace PartnershipManager.Application.Features.Financial.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// FINANCIAL PERIOD
// ────────────────────────────────────────────────────────────────────────────

public record FinancialPeriodResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public short Year { get; init; }
    public byte Month { get; init; }
    public string PeriodLabel { get; init; } = string.Empty;  // e.g. "Jan/2025"
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime? LockedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public FinancialMetricResponse? Metrics { get; init; }
}

public class FinancialPeriodListResponse : PagedResult<FinancialPeriodResponse>
{
    public FinancialPeriodListResponse(IEnumerable<FinancialPeriodResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateFinancialPeriodRequest
{
    public Guid CompanyId { get; init; }
    public short Year { get; init; }
    public byte Month { get; init; }
    public string? Notes { get; init; }
}

public record UpdateFinancialPeriodRequest
{
    public string? Notes { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// FINANCIAL METRICS
// ────────────────────────────────────────────────────────────────────────────

public record FinancialMetricResponse
{
    public Guid Id { get; init; }
    public Guid PeriodId { get; init; }

    // Revenue
    public decimal? GrossRevenue { get; init; }
    public decimal? NetRevenue { get; init; }
    public decimal? Mrr { get; init; }
    public decimal? Arr { get; init; }

    // Cash & Burn
    public decimal? CashBalance { get; init; }
    public decimal? BurnRate { get; init; }
    public decimal? RunwayMonths { get; init; }
    public string? RunwayStatus { get; init; }

    // Unit Economics
    public int? CustomerCount { get; init; }
    public decimal? ChurnRate { get; init; }
    public decimal? Cac { get; init; }
    public decimal? Ltv { get; init; }
    public decimal? LtvToCacRatio { get; init; }
    public short? Nps { get; init; }

    // Profitability
    public decimal? Ebitda { get; init; }
    public decimal? EbitdaMargin { get; init; }
    public decimal? NetIncome { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UpsertRevenueRequest
{
    public decimal? GrossRevenue { get; init; }
    public decimal? NetRevenue { get; init; }
    public decimal? Mrr { get; init; }
}

public record UpsertCashBurnRequest
{
    public decimal? CashBalance { get; init; }
    public decimal? BurnRate { get; init; }
}

public record UpsertUnitEconomicsRequest
{
    public int? CustomerCount { get; init; }
    public decimal? ChurnRate { get; init; }
    public decimal? Cac { get; init; }
    public decimal? Ltv { get; init; }
    public short? Nps { get; init; }
}

public record UpsertProfitabilityRequest
{
    public decimal? Ebitda { get; init; }
    public decimal? NetIncome { get; init; }
}

public record FinancialDashboardResponse
{
    public Guid CompanyId { get; init; }
    public int Year { get; init; }
    public IReadOnlyList<FinancialPeriodResponse> Periods { get; init; } = [];
    public FinancialTrendResponse? Trend { get; init; }
}

public record FinancialTrendResponse
{
    public decimal? MrrGrowthPercent { get; init; }       // vs prior month
    public decimal? ArrCurrentMonth { get; init; }
    public decimal? AvgBurnRate3Months { get; init; }
    public decimal? RunwayMonths { get; init; }
    public string? RunwayStatus { get; init; }
    public decimal? AvgChurnRate3Months { get; init; }
}
