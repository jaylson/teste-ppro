using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// A performance milestone attached to an individual VestingGrant.
/// Progress is tracked incrementally via MilestoneProgress records.
/// When achieved and approved, a VestingAcceleration can be applied.
/// </summary>
public class GrantMilestone : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid VestingGrantId { get; private set; }
    public Guid? MilestoneTemplateId { get; private set; }
    public Guid CompanyId { get; private set; }

    // Definition
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MilestoneCategory Category { get; private set; }
    public MetricType MetricType { get; private set; }
    public decimal TargetValue { get; private set; }
    public TargetOperator TargetOperator { get; private set; }
    public DateTime TargetDate { get; private set; }
    public MeasurementFrequency MeasurementFrequency { get; private set; }

    // Progress tracking
    public MilestoneStatus Status { get; private set; }
    public decimal? CurrentValue { get; private set; }
    public decimal ProgressPercentage { get; private set; }

    // Achievement data
    public DateTime? AchievedAt { get; private set; }
    public decimal? AchievedValue { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedBy { get; private set; }

    // Acceleration settings
    public VestingAccelerationType AccelerationType { get; private set; }
    public decimal AccelerationAmount { get; private set; }
    public bool AccelerationApplied { get; private set; }
    public DateTime? AccelerationAppliedAt { get; private set; }

    // Navigation
    public virtual VestingGrant? Grant { get; private set; }
    public virtual MilestoneTemplate? Template { get; private set; }
    public virtual ICollection<MilestoneProgress> ProgressHistory { get; private set; } = new List<MilestoneProgress>();
    public virtual VestingAcceleration? Acceleration { get; private set; }

    // Derived helpers
    public bool IsAchieved => Status == MilestoneStatus.Achieved;
    public bool IsExpired => Status == MilestoneStatus.Pending && DateTime.UtcNow.Date > TargetDate.Date;
    public bool CanApplyAcceleration => IsAchieved && VerifiedAt.HasValue && !AccelerationApplied;

    private GrantMilestone() { }

    public static GrantMilestone Create(
        Guid clientId,
        Guid vestingGrantId,
        Guid companyId,
        string name,
        MilestoneCategory category,
        MetricType metricType,
        decimal targetValue,
        TargetOperator targetOperator,
        DateTime targetDate,
        MeasurementFrequency measurementFrequency,
        VestingAccelerationType accelerationType,
        decimal accelerationAmount,
        string? description = null,
        Guid? milestoneTemplateId = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do milestone é obrigatório.", nameof(name));
        if (targetValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetValue), "TargetValue deve ser positivo.");
        if (accelerationAmount <= 0 || accelerationAmount > 100)
            throw new ArgumentOutOfRangeException(nameof(accelerationAmount), "AccelerationAmount deve ser entre 0 e 100.");
        if (targetDate.Date <= DateTime.UtcNow.Date)
            throw new ArgumentOutOfRangeException(nameof(targetDate), "Data alvo deve ser no futuro.");

        return new GrantMilestone
        {
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            MilestoneTemplateId = milestoneTemplateId,
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = category,
            MetricType = metricType,
            TargetValue = targetValue,
            TargetOperator = targetOperator,
            TargetDate = targetDate.Date,
            MeasurementFrequency = measurementFrequency,
            Status = MilestoneStatus.Pending,
            ProgressPercentage = 0,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            AccelerationApplied = false,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    // ─── Business Logic ─────────────────────────────────────────────────────

    /// <summary>
    /// Records a new progress value and recalculates the percentage.
    /// Automatically transitions to InProgress when progress > 0.
    /// </summary>
    public decimal RecordProgress(decimal value, Guid updatedBy)
    {
        if (Status == MilestoneStatus.Achieved || Status == MilestoneStatus.Cancelled)
            throw new InvalidOperationException($"Não é possível registrar progresso em milestone com status '{Status}'.");

        CurrentValue = value;
        ProgressPercentage = CalculateProgress(value);

        if (Status == MilestoneStatus.Pending && ProgressPercentage > 0)
            Status = MilestoneStatus.InProgress;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
        return ProgressPercentage;
    }

    /// <summary>Marks the milestone as achieved pending approval.</summary>
    public void MarkAsAchieved(decimal achievedValue, Guid achievedBy)
    {
        if (Status == MilestoneStatus.Achieved)
            throw new InvalidOperationException("Milestone já foi atingido.");
        if (Status == MilestoneStatus.Cancelled || Status == MilestoneStatus.Failed)
            throw new InvalidOperationException($"Milestone '{Status}' não pode ser marcado como atingido.");

        Status = MilestoneStatus.Achieved;
        AchievedAt = DateTime.UtcNow;
        AchievedValue = achievedValue;
        CurrentValue = achievedValue;
        ProgressPercentage = 100m;
        UpdatedBy = achievedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Approves (verifies) the achievement — required before acceleration can be applied.</summary>
    public void Verify(Guid verifiedBy)
    {
        if (Status != MilestoneStatus.Achieved)
            throw new InvalidOperationException("Apenas milestones atingidos podem ser verificados.");
        if (VerifiedAt.HasValue)
            throw new InvalidOperationException("Milestone já verificado.");

        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        UpdatedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marks the acceleration as applied — called by VestingAccelerationEngine.</summary>
    public void MarkAccelerationApplied(Guid appliedBy)
    {
        if (!CanApplyAcceleration)
            throw new InvalidOperationException("Aceleração não pode ser aplicada neste momento.");

        AccelerationApplied = true;
        AccelerationAppliedAt = DateTime.UtcNow;
        UpdatedBy = appliedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(Guid updatedBy)
    {
        if (Status != MilestoneStatus.Pending && Status != MilestoneStatus.InProgress)
            throw new InvalidOperationException($"Milestone no status '{Status}' não pode ser marcado como falho.");

        Status = MilestoneStatus.Failed;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid updatedBy)
    {
        if (Status == MilestoneStatus.Achieved)
            throw new InvalidOperationException("Milestone atingido não pode ser cancelado.");

        Status = MilestoneStatus.Cancelled;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private decimal CalculateProgress(decimal currentValue)
    {
        if (TargetValue == 0) return 0m;

        var raw = TargetOperator switch
        {
            TargetOperator.LessThan or TargetOperator.LessThanOrEqual =>
                (TargetValue / Math.Max(currentValue, 0.0001m)) * 100m,
            _ => (currentValue / TargetValue) * 100m
        };

        return Math.Min(Math.Max(Math.Round(raw, 2), 0m), 100m);
    }

    /// <summary>Reconstitutes from persistence without domain validation. For repositories only.</summary>
    public static GrantMilestone Reconstitute(
        Guid id, Guid clientId, Guid vestingGrantId, Guid? milestoneTemplateId, Guid companyId,
        string name, string? description,
        MilestoneCategory category, MetricType metricType,
        decimal targetValue, TargetOperator targetOperator, DateTime targetDate,
        MeasurementFrequency measurementFrequency,
        MilestoneStatus status, decimal? currentValue, decimal progressPercentage,
        DateTime? achievedAt, decimal? achievedValue, DateTime? verifiedAt, Guid? verifiedBy,
        VestingAccelerationType accelerationType, decimal accelerationAmount,
        bool accelerationApplied, DateTime? accelerationAppliedAt,
        Guid? createdBy, DateTime createdAt, DateTime updatedAt,
        bool isDeleted, DateTime? deletedAt)
    {
        return new GrantMilestone
        {
            Id = id,
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            MilestoneTemplateId = milestoneTemplateId,
            CompanyId = companyId,
            Name = name,
            Description = description,
            Category = category,
            MetricType = metricType,
            TargetValue = targetValue,
            TargetOperator = targetOperator,
            TargetDate = targetDate,
            MeasurementFrequency = measurementFrequency,
            Status = status,
            CurrentValue = currentValue,
            ProgressPercentage = progressPercentage,
            AchievedAt = achievedAt,
            AchievedValue = achievedValue,
            VerifiedAt = verifiedAt,
            VerifiedBy = verifiedBy,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            AccelerationApplied = accelerationApplied,
            AccelerationAppliedAt = accelerationAppliedAt,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
