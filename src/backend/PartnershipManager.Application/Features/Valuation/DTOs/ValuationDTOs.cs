using PartnershipManager.Application.Common.Models;

namespace PartnershipManager.Application.Features.Valuation.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// VALUATION
// ────────────────────────────────────────────────────────────────────────────

public record ValuationResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public DateTime ValuationDate { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? EventName { get; init; }
    public decimal? ValuationAmount { get; init; }
    public decimal TotalShares { get; init; }
    public decimal? PricePerShare { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime? RejectedAt { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<ValuationMethodResponse> Methods { get; init; } = [];
}

public class ValuationListResponse : PagedResult<ValuationResponse>
{
    public ValuationListResponse(IEnumerable<ValuationResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateValuationRequest
{
    public Guid CompanyId { get; init; }
    public DateTime ValuationDate { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? EventName { get; init; }
    public decimal TotalShares { get; init; }
    public string? Notes { get; init; }
}

public record UpdateValuationRequest
{
    public DateTime ValuationDate { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? EventName { get; init; }
    public decimal TotalShares { get; init; }
    public string? Notes { get; init; }
}

public record SubmitValuationRequest
{
    public Guid ValuationId { get; init; }
}

public record ApproveValuationRequest
{
    public Guid ValuationId { get; init; }
}

public record RejectValuationRequest
{
    public Guid ValuationId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record ReturnToDraftRequest
{
    public Guid ValuationId { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// VALUATION METHOD
// ────────────────────────────────────────────────────────────────────────────

public record ValuationMethodResponse
{
    public Guid Id { get; init; }
    public Guid ValuationId { get; init; }
    public string MethodType { get; init; } = string.Empty;
    public bool IsSelected { get; init; }
    public decimal? CalculatedValue { get; init; }
    public string? Inputs { get; init; }
    public string? DataSource { get; init; }
    public string? Notes { get; init; }
    public Guid? FormulaVersionId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record AddValuationMethodRequest
{
    public string MethodType { get; init; } = string.Empty;
    public string? InputsJson { get; init; }
    public string? DataSource { get; init; }
    public string? Notes { get; init; }
    public Guid? FormulaVersionId { get; init; }
}

public record CalculateMethodRequest
{
    public string MethodType { get; init; } = string.Empty;
    public Dictionary<string, decimal> Inputs { get; init; } = [];
    public Guid? FormulaVersionId { get; init; }
}

public record CalculateMethodResponse
{
    public string MethodType { get; init; } = string.Empty;
    public decimal CalculatedValue { get; init; }
    public Dictionary<string, object> Breakdown { get; init; } = [];
    public string? FormulaExpression { get; init; }
}

public record SelectMethodRequest
{
    public Guid MethodId { get; init; }
}

// ────────────────────────────────────────────────────────────────────────────
// VALUATION DOCUMENT
// ────────────────────────────────────────────────────────────────────────────

public record ValuationDocumentResponse
{
    public Guid Id { get; init; }
    public Guid ValuationId { get; init; }
    public Guid DocumentId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttachValuationDocumentRequest
{
    public Guid DocumentId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
