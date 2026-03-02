using PartnershipManager.Infrastructure.Services.Valuation;

namespace PartnershipManager.Tests.Unit.Application.Services;

public class ValuationCalculationEngineTests
{
    private readonly ValuationCalculationEngine _engine;

    public ValuationCalculationEngineTests()
    {
        var customFormulaEngine = new CustomFormulaEngine();
        _engine = new ValuationCalculationEngine(customFormulaEngine);
    }

    // ─── ARR Multiple ────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_ArrMultiple_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["arr"] = 1_500_000m,
            ["multiple"] = 8m
        };
        var result = await _engine.CalculateAsync("arr_multiple", inputs);
        result.CalculatedValue.Should().Be(12_000_000m);
        result.MethodType.Should().Be("arr_multiple");
    }

    // ─── MRR Multiple ────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_MrrMultiple_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["mrr"] = 125_000m,
            ["multiple"] = 12m
        };
        var result = await _engine.CalculateAsync("mrr_multiple", inputs);
        result.CalculatedValue.Should().Be(1_500_000m);
    }

    // ─── EBITDA Multiple ─────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_EbitdaMultiple_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["ebitda"] = 2_000_000m,
            ["multiple"] = 5m
        };
        var result = await _engine.CalculateAsync("ebitda_multiple", inputs);
        result.CalculatedValue.Should().Be(10_000_000m);
    }

    // ─── Asset Based ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_AssetBased_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["total_assets"] = 5_000_000m,
            ["total_liabilities"] = 2_000_000m
        };
        var result = await _engine.CalculateAsync("asset_based", inputs);
        result.CalculatedValue.Should().Be(3_000_000m);
    }

    // ─── Comparables ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_Comparables_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["revenue"] = 3_000_000m,
            ["multiple"] = 4m
        };
        var result = await _engine.CalculateAsync("comparables", inputs);
        result.CalculatedValue.Should().Be(12_000_000m);
    }

    // ─── DCF ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_Dcf_ReturnsPositiveValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["annual_cash_flow"] = 1_000_000m,
            ["growth_rate"] = 10m,     // 10%
            ["discount_rate"] = 15m    // 15%
        };
        var result = await _engine.CalculateAsync("dcf", inputs);
        result.CalculatedValue.Should().BeGreaterThan(0);
    }

    // ─── Berkus ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_Berkus_WithNoInputs_ReturnsZero()
    {
        var inputs = new Dictionary<string, decimal>();
        var result = await _engine.CalculateAsync("berkus", inputs);
        result.CalculatedValue.Should().Be(0m);
    }

    [Fact]
    public async Task Calculate_Berkus_WithSomeComponents_ReturnsSumOfComponents()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["sound_idea"] = 500_000m,
            ["prototype"] = 500_000m,
            ["management_team"] = 500_000m
        };
        var result = await _engine.CalculateAsync("berkus", inputs);
        result.CalculatedValue.Should().Be(1_500_000m);
    }

    // ─── Custom ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Calculate_Custom_WithFormulaExpression_ReturnsCorrectValue()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["hectares"] = 100m,
            ["preco"] = 200m
        };
        var result = await _engine.CalculateAsync("custom", inputs, "[hectares] * [preco]");
        result.CalculatedValue.Should().Be(20_000m);
    }

    [Fact]
    public async Task Calculate_Custom_WithoutFormulaExpression_Throws()
    {
        var inputs = new Dictionary<string, decimal> { ["a"] = 1m };
        var act = async () => await _engine.CalculateAsync("custom", inputs); // no expression
        await act.Should().ThrowAsync<Exception>();
    }

    // ─── Validation ──────────────────────────────────────────────────────────

    [Fact]
    public void ValidateInputs_ArrMultiple_MissingRequired_ReturnsErrors()
    {
        var inputs = new Dictionary<string, decimal>(); // missing arr + multiple
        var errors = _engine.ValidateInputs("arr_multiple", inputs);
        errors.Should().Contain(e => e.Contains("arr"));
        errors.Should().Contain(e => e.Contains("multiple"));
    }

    [Fact]
    public void ValidateInputs_ArrMultiple_AllPresent_ReturnsEmpty()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["arr"] = 1_000_000m,
            ["multiple"] = 5m
        };
        var errors = _engine.ValidateInputs("arr_multiple", inputs);
        errors.Should().BeEmpty();
    }
}
