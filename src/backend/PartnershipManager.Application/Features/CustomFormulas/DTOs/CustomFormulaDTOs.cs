using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Application.Features.CustomFormulas.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// CUSTOM FORMULA
// ────────────────────────────────────────────────────────────────────────────

public record CustomFormulaResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SectorTag { get; init; }
    public Guid? CurrentVersionId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public FormulaVersionResponse? CurrentVersion { get; init; }
}

public class CustomFormulaListResponse : PagedResult<CustomFormulaResponse>
{
    public CustomFormulaListResponse(IEnumerable<CustomFormulaResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateCustomFormulaRequest
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SectorTag { get; init; }
    // Initial version (required at creation time)
    public string Expression { get; init; } = string.Empty;
    public List<FormulaVariableDefinition> Variables { get; init; } = [];
    public string ResultUnit { get; init; } = "BRL";
    public string? ResultLabel { get; init; }
}

public record UpdateFormulaMetadataRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SectorTag { get; init; }
}

public record PublishNewFormulaVersionRequest
{
    public string Expression { get; init; } = string.Empty;
    public List<FormulaVariableDefinition> Variables { get; init; } = [];
    public string ResultUnit { get; init; } = "BRL";
    public string? ResultLabel { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// FORMULA VERSION
// ────────────────────────────────────────────────────────────────────────────

public record FormulaVersionResponse
{
    public Guid Id { get; init; }
    public Guid FormulaId { get; init; }
    public int VersionNumber { get; init; }
    public string Expression { get; init; } = string.Empty;
    public List<FormulaVariableDefinition> Variables { get; init; } = [];
    public string ResultUnit { get; init; } = string.Empty;
    public string? ResultLabel { get; init; }
    public string? TestInputs { get; init; }
    public decimal? TestResult { get; init; }
    public string ValidationStatus { get; init; } = string.Empty;
    public string? ValidationErrors { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// FORMULA EXECUTION
// ────────────────────────────────────────────────────────────────────────────

public record FormulaExecutionResponse
{
    public Guid Id { get; init; }
    public Guid ValuationMethodId { get; init; }
    public Guid FormulaVersionId { get; init; }
    public string InputsUsed { get; init; } = "{}";
    public decimal CalculatedValue { get; init; }
    public string ExpressionSnapshot { get; init; } = string.Empty;
    public DateTime ExecutedAt { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// FORMULA TESTING (dry-run before saving version)
// ────────────────────────────────────────────────────────────────────────────

public record TestFormulaRequest
{
    public string Expression { get; init; } = string.Empty;
    public Dictionary<string, decimal> Inputs { get; init; } = [];
}

public record TestFormulaResponse
{
    public bool IsValid { get; init; }
    public decimal? Result { get; init; }
    public List<string> Errors { get; init; } = [];
    public string? NormalizedExpression { get; init; }
}
