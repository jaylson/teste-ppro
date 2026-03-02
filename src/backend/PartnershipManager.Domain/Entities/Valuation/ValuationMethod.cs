namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Represents one calculation methodology applied to a valuation.
/// Business Rule VA-01: Exactly one per valuation must have IsSelected = true.
/// When method_type = 'custom', FormulaVersionId must be set.
/// </summary>
public class ValuationMethod : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid ValuationId { get; private set; }

    public string MethodType { get; private set; } = string.Empty;
    public bool IsSelected { get; private set; }

    public decimal? CalculatedValue { get; private set; }

    /// <summary>JSON object with methodology-specific inputs (e.g. {arr: 1500000, multiple: 8}).</summary>
    public string? Inputs { get; private set; }
    public string? DataSource { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Set only when MethodType = 'custom'. References the exact formula version used.</summary>
    public Guid? FormulaVersionId { get; private set; }

    // Derived
    public bool IsCustomFormula => MethodType == ValuationMethodTypes.Custom;

    private ValuationMethod() { }

    public static ValuationMethod Create(
        Guid clientId,
        Guid valuationId,
        string methodType,
        string? inputsJson = null,
        string? dataSource = null,
        string? notes = null,
        Guid? formulaVersionId = null,
        Guid? createdBy = null)
    {
        if (!ValuationMethodTypes.All.Contains(methodType))
            throw new ArgumentException($"Tipo de metodologia inválido: {methodType}.", nameof(methodType));
        if (methodType == ValuationMethodTypes.Custom && !formulaVersionId.HasValue)
            throw new ArgumentException("FormulaVersionId é obrigatório para metodologia do tipo 'custom'.", nameof(formulaVersionId));

        var m = new ValuationMethod
        {
            ClientId = clientId,
            ValuationId = valuationId,
            MethodType = methodType,
            IsSelected = false,
            Inputs = inputsJson,
            DataSource = dataSource?.Trim(),
            Notes = notes?.Trim(),
            FormulaVersionId = formulaVersionId
        };

        m.CreatedBy = createdBy;
        m.UpdatedBy = createdBy;
        return m;
    }

    public void SetCalculatedValue(decimal value, Guid updatedBy)
    {
        CalculatedValue = value;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Select(Guid updatedBy)
    {
        IsSelected = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deselect(Guid updatedBy)
    {
        IsSelected = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInputs(string? inputsJson, string? dataSource, string? notes, Guid updatedBy)
    {
        Inputs = inputsJson;
        DataSource = dataSource?.Trim();
        Notes = notes?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>Valid method types for valuation calculations.</summary>
public static class ValuationMethodTypes
{
    public const string ArrMultiple = "arr_multiple";
    public const string Dcf = "dcf";
    public const string Comparables = "comparables";
    public const string EbitdaMultiple = "ebitda_multiple";
    public const string MrrMultiple = "mrr_multiple";
    public const string AssetBased = "asset_based";
    public const string Berkus = "berkus";
    public const string Custom = "custom";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        ArrMultiple, Dcf, Comparables, EbitdaMultiple,
        MrrMultiple, AssetBased, Berkus, Custom
    };
}
