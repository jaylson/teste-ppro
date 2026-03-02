using PartnershipManager.Infrastructure.Services.Valuation;

namespace PartnershipManager.Tests.Unit.Application.Services;

public class CustomFormulaEngineTests
{
    private readonly CustomFormulaEngine _engine = new();

    // ─── Evaluate ────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_SimpleExpression_ReturnsCorrectResult()
    {
        var inputs = new Dictionary<string, decimal> { ["a"] = 3m, ["b"] = 4m };
        var result = _engine.Evaluate("[a] + [b]", inputs);
        result.Should().Be(7m);
    }

    [Fact]
    public void Evaluate_MultiplicationExpression_ReturnsCorrectResult()
    {
        var inputs = new Dictionary<string, decimal> { ["arr"] = 1_500_000m, ["multiple"] = 8m };
        var result = _engine.Evaluate("[arr] * [multiple]", inputs);
        result.Should().Be(12_000_000m);
    }

    [Fact]
    public void Evaluate_ComplexExpression_ReturnsCorrectResult()
    {
        var inputs = new Dictionary<string, decimal>
        {
            ["hectares"] = 100m,
            ["preco_saca"] = 50m,
            ["sacas_por_hectare"] = 60m
        };
        var result = _engine.Evaluate("[hectares] * [preco_saca] * [sacas_por_hectare]", inputs);
        result.Should().Be(300_000m);
    }

    [Fact]
    public void Evaluate_DivisionExpression_ReturnsCorrectResult()
    {
        var inputs = new Dictionary<string, decimal> { ["total"] = 100m, ["count"] = 4m };
        var result = _engine.Evaluate("[total] / [count]", inputs);
        result.Should().Be(25m);
    }

    // ─── Security blocks ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("System.Console.WriteLine('hack')")]
    [InlineData("Process.Start('cmd')")]
    [InlineData("Assembly.Load('evil')")]
    [InlineData("File.ReadAllText('/etc/passwd')")]
    [InlineData("Environment.Exit(0)")]
    public void Evaluate_ForbiddenKeyword_ThrowsDomainException(string expression)
    {
        var inputs = new Dictionary<string, decimal>();
        var act = () => _engine.Evaluate(expression, inputs);
        act.Should().Throw<Exception>()
            .WithMessage("*bloqueado*");
    }

    [Fact]
    public void Evaluate_ExpressionWithDoubleQuotes_ThrowsDomainException()
    {
        var inputs = new Dictionary<string, decimal>();
        var act = () => _engine.Evaluate("[a] + \"evil\"", inputs);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Evaluate_TooLongExpression_ThrowsDomainException()
    {
        var longExpr = string.Concat(Enumerable.Repeat("[a] + ", 400)) + "[a]";
        var inputs = new Dictionary<string, decimal> { ["a"] = 1m };
        var act = () => _engine.Evaluate(longExpr, inputs);
        act.Should().Throw<Exception>();
    }

    // ─── TryValidate ─────────────────────────────────────────────────────────

    [Fact]
    public void TryValidate_ValidExpression_ReturnsTrue()
    {
        var isValid = _engine.TryValidate("[a] * [b]", out var errors);
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_BlockedKeyword_ReturnsFalse()
    {
        var isValid = _engine.TryValidate("System.Console.WriteLine('x')", out var errors);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void TryValidate_SyntaxError_ReturnsFalse()
    {
        // Unbalanced parenthesis
        var isValid = _engine.TryValidate("[a] * ((([b])", out var errors);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void TryValidate_EmptyExpression_ReturnsFalse()
    {
        var isValid = _engine.TryValidate("", out var errors);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }
}
