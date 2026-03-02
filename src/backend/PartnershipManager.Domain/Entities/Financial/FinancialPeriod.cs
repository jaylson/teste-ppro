namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Monthly financial data container for a company.
/// Workflow: draft → submitted → approved → locked
/// Business Rules:
///   FI-01: One period per company per (year, month) — enforced at DB level (UNIQUE KEY)
///   FI-02: Cannot approve a period if the previous consecutive period is not approved (service layer)
///   FI-03: Approved/locked period cannot be edited (enforced here)
///   FI-04: Required documents must exist before period can be submitted (service layer)
/// </summary>
public class FinancialPeriod : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }

    public short Year { get; private set; }
    public byte Month { get; private set; }

    public string Status { get; private set; } = FinancialPeriodStatus.Draft;
    public string? Notes { get; private set; }

    // Workflow
    public DateTime? SubmittedAt { get; private set; }
    public Guid? SubmittedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? LockedAt { get; private set; }
    public Guid? LockedBy { get; private set; }

    // Derived helpers
    public bool IsDraft => Status == FinancialPeriodStatus.Draft;
    public bool IsSubmitted => Status == FinancialPeriodStatus.Submitted;
    public bool IsApproved => Status == FinancialPeriodStatus.Approved;
    public bool IsLocked => Status == FinancialPeriodStatus.Locked;
    /// <summary>FI-03: locked periods cannot be edited.</summary>
    public bool CanBeEdited => IsDraft || IsSubmitted;
    public string PeriodLabel => $"{Year:D4}/{Month:D2}";

    private FinancialPeriod() { }

    public static FinancialPeriod Create(
        Guid clientId,
        Guid companyId,
        short year,
        byte month,
        string? notes = null,
        Guid? createdBy = null)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), "Ano inválido.");
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Mês deve ser entre 1 e 12.");

        var fp = new FinancialPeriod
        {
            ClientId = clientId,
            CompanyId = companyId,
            Year = year,
            Month = month,
            Status = FinancialPeriodStatus.Draft,
            Notes = notes?.Trim()
        };

        fp.CreatedBy = createdBy;
        fp.UpdatedBy = createdBy;
        return fp;
    }

    public void UpdateNotes(string? notes, Guid updatedBy)
    {
        EnsureEditable();
        Notes = notes?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit(Guid submittedBy)
    {
        if (!IsDraft)
            throw new InvalidOperationException($"Período {PeriodLabel} não pode ser submetido — status atual: {Status}.");

        Status = FinancialPeriodStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        SubmittedBy = submittedBy;
        UpdatedBy = submittedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedBy)
    {
        if (!IsSubmitted)
            throw new InvalidOperationException($"Período {PeriodLabel} não pode ser aprovado — status atual: {Status}.");

        Status = FinancialPeriodStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        UpdatedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>FI-03: Lock seals the period permanently — no further edits allowed.</summary>
    public void Lock(Guid lockedBy)
    {
        if (!IsApproved)
            throw new InvalidOperationException($"Apenas períodos aprovados podem ser bloqueados.");

        Status = FinancialPeriodStatus.Locked;
        LockedAt = DateTime.UtcNow;
        LockedBy = lockedBy;
        UpdatedBy = lockedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Allows returning an approved period to submitted for correction (before locking).</summary>
    public void ReturnToSubmitted(Guid updatedBy)
    {
        if (!IsApproved)
            throw new InvalidOperationException("Apenas períodos aprovados podem ser retornados para revisão.");

        Status = FinancialPeriodStatus.Submitted;
        ApprovedAt = null;
        ApprovedBy = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (!CanBeEdited)
            throw new InvalidOperationException($"Período {PeriodLabel} está bloqueado e não pode ser editado (FI-03).");
    }
}

public static class FinancialPeriodStatus
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Approved = "approved";
    public const string Locked = "locked";
}
