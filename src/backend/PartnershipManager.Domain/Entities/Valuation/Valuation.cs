namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Represents one valuation event for a company (e.g. Seed Round, Series A, internal 409A).
/// Workflow: draft → pending_approval → approved | rejected
/// Business Rules:
///   VA-01: Exactly one ValuationMethod must have IsSelected = true
///   VA-02: Approval workflow required before valuation becomes official
///   VA-03: ValuationDate must be posterior to last approved valuation
///   VA-04: PricePerShare = ValuationAmount / TotalShares (computed on approval)
/// </summary>
public class Valuation : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }

    // Event info
    public DateTime ValuationDate { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? EventName { get; private set; }

    // Calculated result
    public decimal? ValuationAmount { get; private set; }
    public decimal TotalShares { get; private set; }
    public decimal? PricePerShare { get; private set; }

    // Workflow
    public string Status { get; private set; } = ValuationStatus.Draft;
    public string? Notes { get; private set; }

    // Approval
    public DateTime? SubmittedAt { get; private set; }
    public Guid? SubmittedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    // Derived helpers
    public bool IsDraft => Status == ValuationStatus.Draft;
    public bool IsPendingApproval => Status == ValuationStatus.PendingApproval;
    public bool IsApproved => Status == ValuationStatus.Approved;
    public bool IsRejected => Status == ValuationStatus.Rejected;
    public bool CanBeEdited => IsDraft;

    private Valuation() { }

    public static Valuation Create(
        Guid clientId,
        Guid companyId,
        DateTime valuationDate,
        string eventType,
        decimal totalShares,
        string? eventName = null,
        string? notes = null,
        Guid? createdBy = null)
    {
        if (totalShares <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalShares), "Total de ações deve ser positivo.");
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Tipo do evento é obrigatório.", nameof(eventType));
        if (!ValuationEventTypes.All.Contains(eventType))
            throw new ArgumentException($"Tipo de evento inválido: {eventType}.", nameof(eventType));

        var v = new Valuation
        {
            ClientId = clientId,
            CompanyId = companyId,
            ValuationDate = valuationDate.Date,
            EventType = eventType,
            EventName = eventName?.Trim(),
            TotalShares = totalShares,
            Status = ValuationStatus.Draft,
            Notes = notes?.Trim()
        };

        v.CreatedBy = createdBy;
        v.UpdatedBy = createdBy;
        return v;
    }

    /// <summary>Sets the official valuation amount from the selected methodology.</summary>
    public void SetValuationAmount(decimal amount, Guid updatedBy)
    {
        if (!CanBeEdited)
            throw new InvalidOperationException("Apenas valuations em rascunho podem ter o valor atualizado.");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Valor do valuation deve ser positivo.");

        ValuationAmount = amount;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        DateTime valuationDate,
        string eventType,
        decimal totalShares,
        string? eventName,
        string? notes,
        Guid updatedBy)
    {
        if (!CanBeEdited)
            throw new InvalidOperationException("Apenas valuations em rascunho podem ser editados.");
        if (totalShares <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalShares));
        if (!ValuationEventTypes.All.Contains(eventType))
            throw new ArgumentException($"Tipo de evento inválido: {eventType}.");

        ValuationDate = valuationDate.Date;
        EventType = eventType;
        EventName = eventName?.Trim();
        TotalShares = totalShares;
        Notes = notes?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit(Guid submittedBy)
    {
        if (!IsDraft)
            throw new InvalidOperationException("Apenas valuations em rascunho podem ser submetidos para aprovação.");
        if (!ValuationAmount.HasValue || ValuationAmount <= 0)
            throw new InvalidOperationException("Valuation sem valor calculado não pode ser submetido. Selecione uma metodologia principal.");

        Status = ValuationStatus.PendingApproval;
        SubmittedAt = DateTime.UtcNow;
        SubmittedBy = submittedBy;
        UpdatedBy = submittedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the valuation and calculates PricePerShare (VA-04).
    /// Cap Table update must be done by the service after calling this.
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        if (!IsPendingApproval)
            throw new InvalidOperationException("Apenas valuations pendentes de aprovação podem ser aprovados.");
        if (!ValuationAmount.HasValue || ValuationAmount <= 0)
            throw new InvalidOperationException("Valor do valuation não definido.");

        // VA-04: price_per_share = valuation_amount / total_shares
        PricePerShare = Math.Round(ValuationAmount.Value / TotalShares, 6);

        Status = ValuationStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        UpdatedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(Guid rejectedBy, string reason)
    {
        if (!IsPendingApproval)
            throw new InvalidOperationException("Apenas valuations pendentes de aprovação podem ser rejeitados.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Motivo da rejeição é obrigatório.", nameof(reason));

        Status = ValuationStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectedBy = rejectedBy;
        RejectionReason = reason.Trim();
        UpdatedBy = rejectedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Allows a rejected valuation to return to draft for correction.</summary>
    public void ReturnToDraft(Guid updatedBy)
    {
        if (!IsRejected)
            throw new InvalidOperationException("Apenas valuations rejeitados podem ser retornados para rascunho.");

        Status = ValuationStatus.Draft;
        RejectedAt = null;
        RejectedBy = null;
        RejectionReason = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>Valuation workflow status constants.</summary>
public static class ValuationStatus
{
    public const string Draft = "draft";
    public const string PendingApproval = "pending_approval";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

/// <summary>Valid event types for a valuation.</summary>
public static class ValuationEventTypes
{
    public const string Founding = "founding";
    public const string Seed = "seed";
    public const string SeriesA = "series_a";
    public const string SeriesB = "series_b";
    public const string SeriesC = "series_c";
    public const string Internal = "internal";
    public const string External = "external";
    public const string FourNineA = "409a";
    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Founding, Seed, SeriesA, SeriesB, SeriesC,
        Internal, External, FourNineA, Other
    };
}
