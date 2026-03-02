using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Tests.Unit.Domain.Financial;

public class FinancialMetricTests
{
    // ──────────────────────────────────────────────
    // ARR auto-calculation: ARR = MRR × 12
    // ──────────────────────────────────────────────

    [Fact]
    public void UpdateRevenue_ShouldAutoCalculateArr()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.UpdateRevenue(null, null, mrr: 50_000m, updatedBy: Guid.NewGuid());

        m.Mrr.Should().Be(50_000m);
        m.Arr.Should().Be(600_000m); // 50_000 × 12
    }

    [Fact]
    public void UpdateRevenue_WithNullMrr_ShouldSetArrToNull()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.UpdateRevenue(null, null, mrr: null, updatedBy: Guid.NewGuid());

        m.Arr.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // Runway: RunwayMonths = CashBalance / BurnRate
    // ──────────────────────────────────────────────

    [Fact]
    public void UpdateCashBurn_ShouldAutoCalculateRunway()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.UpdateCashBurn(cashBalance: 300_000m, burnRate: 30_000m, updatedBy: Guid.NewGuid());

        m.RunwayMonths.Should().Be(10m); // 300_000 / 30_000
    }

    [Fact]
    public void UpdateCashBurn_WithZeroBurnRate_ShouldSetRunwayToNull()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.UpdateCashBurn(cashBalance: 300_000m, burnRate: 0m, updatedBy: Guid.NewGuid());

        m.RunwayMonths.Should().BeNull();
    }

    [Fact]
    public void UpdateCashBurn_WithNullBurnRate_ShouldSetRunwayToNull()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.UpdateCashBurn(cashBalance: 300_000m, burnRate: null, updatedBy: Guid.NewGuid());

        m.RunwayMonths.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // RunwayStatus — semáforo: >=12=green, >=6=yellow, <6=red
    // ──────────────────────────────────────────────

    [Fact]
    public void RunwayStatus_WhenAboveOrEqual12Months_ShouldBeGreen()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        m.UpdateCashBurn(cashBalance: 1_200_000m, burnRate: 100_000m, updatedBy: Guid.NewGuid());
        // Runway = 12 meses

        m.RunwayStatus.Should().Be("green");
    }

    [Fact]
    public void RunwayStatus_Between6And12Months_ShouldBeYellow()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        m.UpdateCashBurn(cashBalance: 800_000m, burnRate: 100_000m, updatedBy: Guid.NewGuid());
        // Runway = 8 meses

        m.RunwayStatus.Should().Be("yellow");
    }

    [Fact]
    public void RunwayStatus_Below6Months_ShouldBeRed()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        m.UpdateCashBurn(cashBalance: 300_000m, burnRate: 100_000m, updatedBy: Guid.NewGuid());
        // Runway = 3 meses

        m.RunwayStatus.Should().Be("red");
    }

    [Fact]
    public void RunwayStatus_WhenNull_ShouldBeNull()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // sem updateCashBurn

        m.RunwayStatus.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // EbitdaMargin: EBITDA / NetRevenue × 100
    // ──────────────────────────────────────────────

    [Fact]
    public void EbitdaMargin_ShouldBeCalculatedFromNetRevenue()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // Primeiro define NetRevenue via UpdateRevenue
        m.UpdateRevenue(null, netRevenue: 200_000m, mrr: null, updatedBy: Guid.NewGuid());

        // Depois define EBITDA via UpdateProfitability (calcula margem com NetRevenue já definido)
        m.UpdateProfitability(ebitda: 40_000m, netIncome: null, updatedBy: Guid.NewGuid());

        m.EbitdaMargin.Should().Be(20m); // 40_000/200_000*100
    }

    [Fact]
    public void EbitdaMargin_WhenNetRevenueIsNull_ShouldBeNull()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // NetRevenue não foi definido

        m.UpdateProfitability(ebitda: 10_000m, netIncome: null, updatedBy: Guid.NewGuid());

        m.EbitdaMargin.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // Negative value validation
    // ──────────────────────────────────────────────

    [Fact]
    public void UpdateRevenue_WithNegativeMrr_ShouldThrow()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = () => m.UpdateRevenue(null, null, mrr: -100m, updatedBy: Guid.NewGuid());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldDefaultToNullMetrics()
    {
        var m = FinancialMetric.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        m.Mrr.Should().BeNull();
        m.Arr.Should().BeNull();
        m.RunwayMonths.Should().BeNull();
    }
}
