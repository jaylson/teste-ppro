using System.Text.Json;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// IMMUTABLE snapshot of a custom formula version.
/// Once created, this row is never updated (append-only).
/// Stores the expression and variable definitions as JSON.
/// Valuations reference this version ID permanently for audit integrity.
/// </summary>
public class ValuationFormulaVersion
{
    public Guid Id { get; private set; }
    public Guid FormulaId { get; private set; }
    public Guid ClientId { get; private set; }

    public int VersionNumber { get; private set; }

    /// <summary>NCalc2 expression, e.g. "[hectares] * [preco_saca] * [sacas_por_hectare]"</summary>
    public string Expression { get; private set; } = string.Empty;

    /// <summary>JSON array of VariableDefinition objects.</summary>
    public string Variables { get; private set; } = "[]";

    public string ResultUnit { get; private set; } = "BRL";
    public string? ResultLabel { get; private set; }

    // Test data (populated when tester runs before saving)
    public string? TestInputs { get; private set; }
    public decimal? TestResult { get; private set; }

    // Validation
    public string ValidationStatus { get; private set; } = FormulaValidationStatus.Draft;
    public string? ValidationErrors { get; private set; }

    // Immutable — only created_at (no updated_at)
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Derived helpers
    public bool IsValidated => ValidationStatus == FormulaValidationStatus.Validated;
    public bool IsInvalid => ValidationStatus == FormulaValidationStatus.Invalid;

    private ValuationFormulaVersion() { }

    public static ValuationFormulaVersion Create(
        Guid formulaId,
        Guid clientId,
        int versionNumber,
        string expression,
        string variablesJson,
        string resultUnit,
        string? resultLabel,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expressão da fórmula é obrigatória.", nameof(expression));
        if (string.IsNullOrWhiteSpace(variablesJson))
            throw new ArgumentException("Variáveis da fórmula são obrigatórias.", nameof(variablesJson));
        if (versionNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(versionNumber), "Número da versão deve ser positivo.");

        return new ValuationFormulaVersion
        {
            Id = Guid.NewGuid(),
            FormulaId = formulaId,
            ClientId = clientId,
            VersionNumber = versionNumber,
            Expression = expression.Trim(),
            Variables = variablesJson,
            ResultUnit = (resultUnit ?? "BRL").Trim(),
            ResultLabel = resultLabel?.Trim(),
            ValidationStatus = FormulaValidationStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>Mark as validated with optional test result. Called by FormulaSecurityValidator.</summary>
    public void MarkValidated(string? testInputsJson = null, decimal? testResult = null)
    {
        ValidationStatus = FormulaValidationStatus.Validated;
        ValidationErrors = null;
        if (testInputsJson != null) TestInputs = testInputsJson;
        if (testResult.HasValue) TestResult = testResult;
    }

    /// <summary>Mark as invalid with error details.</summary>
    public void MarkInvalid(IEnumerable<string> errors)
    {
        ValidationStatus = FormulaValidationStatus.Invalid;
        ValidationErrors = JsonSerializer.Serialize(errors);
    }

    /// <summary>Deserialize variables JSON into typed list.</summary>
    public List<FormulaVariableDefinition> GetVariables()
    {
        return JsonSerializer.Deserialize<List<FormulaVariableDefinition>>(Variables,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<FormulaVariableDefinition>();
    }
}

public static class FormulaValidationStatus
{
    public const string Draft = "draft";
    public const string Validated = "validated";
    public const string Invalid = "invalid";
}

/// <summary>
/// Variable definition stored in the JSON array within ValuationFormulaVersion.Variables.
/// </summary>
public record FormulaVariableDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = FormulaVariableType.Number;
    public string? Unit { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; } = true;
    public decimal? DefaultValue { get; init; }
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public int DisplayOrder { get; init; }
}

public static class FormulaVariableType
{
    public const string Currency = "currency";
    public const string Percentage = "percentage";
    public const string Number = "number";
    public const string Integer = "integer";
    public const string Multiplier = "multiplier";
    public const string Boolean = "boolean";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Currency, Percentage, Number, Integer, Multiplier, Boolean
    };
}
