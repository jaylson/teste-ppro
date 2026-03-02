using NCalc2;
using PartnershipManager.Domain.Exceptions;

namespace PartnershipManager.Infrastructure.Services.Valuation;

/// <summary>
/// Engine responsible for evaluating NCalc2 custom formula expressions.
/// </summary>
public interface ICustomFormulaEngine
{
    /// <summary>Evaluate a NCalc2 expression with the provided variable map. Returns the decimal result.</summary>
    decimal Evaluate(string expression, Dictionary<string, decimal> inputs);

    /// <summary>Try to validate an expression (dry-run with zero inputs). Returns errors if invalid.</summary>
    bool TryValidate(string expression, out string[] errors);
}

/// <summary>
/// Validates a formula expression string against security and syntax rules before execution.
/// </summary>
public static class FormulaSecurityValidator
{
    private static readonly string[] BlockedKeywords =
    {
        "System.", "Process", "Assembly", "AppDomain", "Activator",
        "File", "Directory", "Path", "IO.", "Stream",
        "Reflection", "Type.GetType", "Invoke", "Convert.", "Environment",
        "Thread", "Task.", "GC.", "Marshal", "Unsafe",
        "eval", "exec", "cmd", "shell", "import", "require"
    };

    private const int MaxExpressionLength = 2000;

    /// <summary>Throws <see cref="DomainException"/> if the expression contains forbidden patterns.</summary>
    public static void Validate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new DomainException("Expressão da fórmula não pode estar vazia.");

        if (expression.Length > MaxExpressionLength)
            throw new DomainException($"Expressão excede tamanho máximo de {MaxExpressionLength} caracteres.");

        foreach (var kw in BlockedKeywords)
        {
            if (expression.Contains(kw, StringComparison.OrdinalIgnoreCase))
                throw new DomainException($"Expressão contém padrão bloqueado por segurança: '{kw}'.");
        }

        // Block string-like double quotes (prevents injection of string objects)
        if (expression.Contains('"'))
            throw new DomainException("Expressão não pode conter aspas duplas.");
    }

    /// <summary>Validates that variable names only contain alphanumeric characters and underscores.</summary>
    public static void ValidateVariableNames(IEnumerable<string> variableNames)
    {
        foreach (var name in variableNames)
        {
            if (!IsValidVariableName(name))
                throw new DomainException($"Nome de variável inválido: '{name}'. Apenas letras, números e underscore são permitidos.");
        }
    }

    private static bool IsValidVariableName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        foreach (var c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }
        return true;
    }
}

/// <summary>
/// NCalc2-based implementation of <see cref="ICustomFormulaEngine"/>.
/// </summary>
public class CustomFormulaEngine : ICustomFormulaEngine
{
    public decimal Evaluate(string expression, Dictionary<string, decimal> inputs)
    {
        FormulaSecurityValidator.Validate(expression);

        var expr = new Expression(expression);

        // Inject all variables
        foreach (var (key, value) in inputs)
        {
            expr.Parameters[key] = value;
        }

        object? raw;
        try
        {
            raw = expr.Evaluate();
        }
        catch (Exception ex)
        {
            throw new DomainException($"Erro ao avaliar a fórmula: {ex.Message}");
        }

        if (raw == null)
            throw new DomainException("A fórmula retornou um valor nulo.");

        try
        {
            return Convert.ToDecimal(raw);
        }
        catch
        {
            throw new DomainException($"A fórmula retornou um resultado não numérico: {raw}");
        }
    }

    public bool TryValidate(string expression, out string[] errors)
    {
        var errorList = new List<string>();

        try
        {
            FormulaSecurityValidator.Validate(expression);
        }
        catch (DomainException ex)
        {
            errors = new[] { ex.Message };
            return false;
        }

        // Try parsing with zero values for all detected parameters
        var expr = new Expression(expression);
        expr.EvaluateParameter += (name, args) => args.Result = 0m;

        try
        {
            expr.Evaluate();
        }
        catch (Exception ex)
        {
            errorList.Add($"Erro de sintaxe: {ex.Message}");
        }

        errors = errorList.ToArray();
        return errorList.Count == 0;
    }
}
