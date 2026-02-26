using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

public class VestingGrant : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid VestingPlanId { get; private set; }
    public Guid ShareholderId { get; private set; }
    public Guid CompanyId { get; private set; }

    public DateTime GrantDate { get; private set; }
    public decimal TotalShares { get; private set; }
    public decimal SharePrice { get; private set; }
    public decimal EquityPercentage { get; private set; }

    public DateTime VestingStartDate { get; private set; }
    public DateTime VestingEndDate { get; private set; }
    public DateTime? CliffDate { get; private set; }

    public VestingGrantDetailStatus Status { get; private set; }

    public decimal VestedShares { get; private set; }
    public decimal ExercisedShares { get; private set; }

    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    public string? Notes { get; private set; }

    // Derived helpers
    public decimal AvailableToExercise => VestedShares - ExercisedShares;
    public decimal UnvestedShares => TotalShares - VestedShares;
    public bool IsFullyVested => VestedShares >= TotalShares;
    public bool IsFullyExercised => ExercisedShares >= TotalShares;

    private VestingGrant() { }

    public static VestingGrant Create(
        Guid clientId,
        Guid vestingPlanId,
        Guid shareholderId,
        Guid companyId,
        DateTime grantDate,
        decimal totalShares,
        decimal sharePrice,
        decimal equityPercentage,
        DateTime vestingStartDate,
        int vestingMonths,
        int cliffMonths,
        string? notes = null,
        Guid? createdBy = null)
    {
        if (totalShares <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalShares), "Total de ações deve ser positivo.");
        if (sharePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(sharePrice), "Preço de exercício não pode ser negativo.");
        if (equityPercentage <= 0 || equityPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(equityPercentage), "Percentual de equity deve ser entre 0 e 100.");
        if (vestingMonths < 1)
            throw new ArgumentOutOfRangeException(nameof(vestingMonths));

        var vestingEndDate = vestingStartDate.AddMonths(vestingMonths);
        DateTime? cliffDate = cliffMonths > 0
            ? vestingStartDate.AddMonths(cliffMonths)
            : null;

        return new VestingGrant
        {
            ClientId = clientId,
            VestingPlanId = vestingPlanId,
            ShareholderId = shareholderId,
            CompanyId = companyId,
            GrantDate = grantDate.Date,
            TotalShares = totalShares,
            SharePrice = sharePrice,
            EquityPercentage = equityPercentage,
            VestingStartDate = vestingStartDate.Date,
            VestingEndDate = vestingEndDate.Date,
            CliffDate = cliffDate.HasValue ? cliffDate.Value.Date : null,
            Status = VestingGrantDetailStatus.Pending,
            VestedShares = 0,
            ExercisedShares = 0,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    // ─── Business Logic ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the cliff period has been met as of the given date.
    /// </summary>
    public bool IsCliffMet(DateTime asOfDate)
    {
        if (CliffDate is null) return true; // no cliff
        return asOfDate.Date >= CliffDate.Value.Date;
    }

    /// <summary>
    /// Calculates how many shares have vested linearly as of <paramref name="asOfDate"/>.
    /// Returns 0 if before start date or cliff not met.
    /// Returns TotalShares if after end date.
    /// </summary>
    public decimal CalculateVestedShares(DateTime asOfDate)
    {
        var date = asOfDate.Date;

        if (date < VestingStartDate || !IsCliffMet(date))
            return 0m;

        if (date >= VestingEndDate)
            return TotalShares;

        var totalDays = (VestingEndDate - VestingStartDate).TotalDays;
        var elapsedDays = (date - VestingStartDate).TotalDays;

        return Math.Floor(TotalShares * (decimal)(elapsedDays / totalDays));
    }

    /// <summary>
    /// Calculates the vested percentage (0–100) as of <paramref name="asOfDate"/>.
    /// </summary>
    public decimal CalculateVestedPercentage(DateTime asOfDate)
    {
        if (TotalShares == 0) return 0m;
        return CalculateVestedShares(asOfDate) / TotalShares * 100m;
    }

    /// <summary>
    /// Returns a future projection of vested shares and percentage.
    /// </summary>
    public (decimal VestedShares, decimal VestedPercentage) GetFutureProjection(DateTime projectionDate)
    {
        var vested = CalculateVestedShares(projectionDate);
        var pct = TotalShares > 0 ? vested / TotalShares * 100m : 0m;
        return (vested, pct);
    }

    /// <summary>
    /// Returns true if the shareholder can exercise <paramref name="sharesToExercise"/> shares.
    /// </summary>
    public bool CanExercise(decimal sharesToExercise)
    {
        if (Status != VestingGrantDetailStatus.Active && Status != VestingGrantDetailStatus.Approved)
            return false;
        return sharesToExercise > 0 && sharesToExercise <= AvailableToExercise;
    }

    /// <summary>
    /// Records exercise of shares. Throws if amount exceeds available vested shares.
    /// Caller must have already recalculated VestedShares before calling this.
    /// </summary>
    public void ExerciseShares(decimal shares, Guid userId)
    {
        if (shares <= 0)
            throw new ArgumentOutOfRangeException(nameof(shares), "Quantidade de ações a exercer deve ser positiva.");

        if (shares > AvailableToExercise)
            throw new InvalidOperationException(
                $"Não é possível exercer {shares} ações. Disponíveis: {AvailableToExercise}.");

        ExercisedShares += shares;

        if (ExercisedShares >= TotalShares)
            Status = VestingGrantDetailStatus.Exercised;

        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the VestedShares snapshot based on recalculation result.
    /// </summary>
    public void RecalculateVestedShares(DateTime asOfDate, Guid userId)
    {
        VestedShares = CalculateVestedShares(asOfDate);
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != VestingGrantDetailStatus.Pending)
            throw new InvalidOperationException("Apenas grants pendentes podem ser aprovados.");

        Status = VestingGrantDetailStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        UpdatedBy = approvedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(Guid userId)
    {
        if (Status != VestingGrantDetailStatus.Approved)
            throw new InvalidOperationException("Apenas grants aprovados podem ser ativados.");

        Status = VestingGrantDetailStatus.Active;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid userId)
    {
        if (Status == VestingGrantDetailStatus.Exercised || Status == VestingGrantDetailStatus.Cancelled)
            throw new InvalidOperationException($"Grant no status '{Status}' não pode ser cancelado.");

        Status = VestingGrantDetailStatus.Cancelled;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reconstitutes a VestingGrant from persistence without domain validation.
    /// For use by repositories only.
    /// </summary>
    public static VestingGrant Reconstitute(
        Guid id,
        Guid clientId,
        Guid vestingPlanId,
        Guid shareholderId,
        Guid companyId,
        DateTime grantDate,
        decimal totalShares,
        decimal sharePrice,
        decimal equityPercentage,
        DateTime vestingStartDate,
        DateTime vestingEndDate,
        DateTime? cliffDate,
        VestingGrantDetailStatus status,
        decimal vestedShares,
        decimal exercisedShares,
        DateTime? approvedAt,
        Guid? approvedBy,
        string? notes,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new VestingGrant
        {
            Id = id,
            ClientId = clientId,
            VestingPlanId = vestingPlanId,
            ShareholderId = shareholderId,
            CompanyId = companyId,
            GrantDate = grantDate,
            TotalShares = totalShares,
            SharePrice = sharePrice,
            EquityPercentage = equityPercentage,
            VestingStartDate = vestingStartDate,
            VestingEndDate = vestingEndDate,
            CliffDate = cliffDate,
            Status = status,
            VestedShares = vestedShares,
            ExercisedShares = exercisedShares,
            ApprovedAt = approvedAt,
            ApprovedBy = approvedBy,
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
