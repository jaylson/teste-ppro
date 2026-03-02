namespace PartnershipManager.Domain.Entities;

/// <summary>
/// IMMUTABLE audit log of one formula execution within a valuation method.
/// Records the exact inputs, expression snapshot, and calculated result.
/// Never updated after insertion — append-only for full auditability.
/// </summary>
public class ValuationFormulaExecution
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ValuationMethodId { get; private set; }
    public Guid FormulaVersionId { get; private set; }

    /// <summary>JSON snapshot: { "variable_name": value, ... } — exactly what was computed.</summary>
    public string InputsUsed { get; private set; } = "{}";

    public decimal CalculatedValue { get; private set; }

    /// <summary>Copy of the expression at execution time — extra safety for audit.</summary>
    public string ExpressionSnapshot { get; private set; } = string.Empty;

    public Guid ExecutedBy { get; private set; }
    public DateTime ExecutedAt { get; private set; }

    private ValuationFormulaExecution() { }

    public static ValuationFormulaExecution Create(
        Guid clientId,
        Guid valuationMethodId,
        Guid formulaVersionId,
        string inputsUsedJson,
        decimal calculatedValue,
        string expressionSnapshot,
        Guid executedBy)
    {
        if (string.IsNullOrWhiteSpace(inputsUsedJson))
            throw new ArgumentException("Inputs da execução são obrigatórios.", nameof(inputsUsedJson));
        if (string.IsNullOrWhiteSpace(expressionSnapshot))
            throw new ArgumentException("Snapshot da expressão é obrigatório.", nameof(expressionSnapshot));

        return new ValuationFormulaExecution
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ValuationMethodId = valuationMethodId,
            FormulaVersionId = formulaVersionId,
            InputsUsed = inputsUsedJson,
            CalculatedValue = calculatedValue,
            ExpressionSnapshot = expressionSnapshot.Trim(),
            ExecutedBy = executedBy,
            ExecutedAt = DateTime.UtcNow
        };
    }
}
