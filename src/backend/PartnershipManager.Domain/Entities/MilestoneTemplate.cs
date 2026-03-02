using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Reusable milestone configuration that companies can attach to individual vesting grants.
/// Defines the category, metric, target operator, acceleration type and cap.
/// </summary>
public class MilestoneTemplate : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public MilestoneCategory Category { get; private set; }
    public MetricType MetricType { get; private set; }
    public TargetOperator TargetOperator { get; private set; }

    /// <summary>Default target value pre-filled when applying template to a grant.</summary>
    public decimal? TargetValue { get; private set; }
    /// <summary>Default target unit pre-filled when applying template to a grant.</summary>
    public string? TargetUnit { get; private set; }

    public MeasurementFrequency MeasurementFrequency { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Acceleration settings
    public VestingAccelerationType AccelerationType { get; private set; }
    public decimal AccelerationAmount { get; private set; }

    /// <summary>
    /// Maximum cumulative acceleration capped for grants using this template.
    /// When null, the system uses the default cap of 75%.
    /// </summary>
    public decimal? MaxAccelerationCap { get; private set; }

    // Navigation properties
    public virtual ICollection<GrantMilestone> GrantMilestones { get; private set; } = new List<GrantMilestone>();

    private MilestoneTemplate() { }

    public static MilestoneTemplate Create(
        Guid clientId,
        Guid companyId,
        string name,
        MilestoneCategory category,
        MetricType metricType,
        TargetOperator targetOperator,
        MeasurementFrequency measurementFrequency,
        VestingAccelerationType accelerationType,
        decimal accelerationAmount,
        string? description = null,
        decimal? maxAccelerationCap = null,
        decimal? targetValue = null,
        string? targetUnit = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do template é obrigatório.", nameof(name));
        if (accelerationAmount <= 0 || accelerationAmount > 100)
            throw new ArgumentOutOfRangeException(nameof(accelerationAmount), "AccelerationAmount deve ser entre 0 e 100.");
        if (maxAccelerationCap.HasValue && maxAccelerationCap.Value < accelerationAmount)
            throw new ArgumentOutOfRangeException(nameof(maxAccelerationCap), "Cap máximo não pode ser menor que o valor de aceleração individual.");

        return new MilestoneTemplate
        {
            ClientId = clientId,
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = category,
            MetricType = metricType,
            TargetOperator = targetOperator,
            TargetValue = targetValue,
            TargetUnit = string.IsNullOrWhiteSpace(targetUnit) ? null : targetUnit.Trim(),
            MeasurementFrequency = measurementFrequency,
            IsActive = true,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            MaxAccelerationCap = maxAccelerationCap,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(
        string name,
        MilestoneCategory category,
        MetricType metricType,
        TargetOperator targetOperator,
        MeasurementFrequency measurementFrequency,
        VestingAccelerationType accelerationType,
        decimal accelerationAmount,
        string? description = null,
        decimal? maxAccelerationCap = null,
        decimal? targetValue = null,
        string? targetUnit = null,
        Guid? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do template é obrigatório.", nameof(name));
        if (accelerationAmount <= 0 || accelerationAmount > 100)
            throw new ArgumentOutOfRangeException(nameof(accelerationAmount));
        if (maxAccelerationCap.HasValue && maxAccelerationCap.Value < accelerationAmount)
            throw new ArgumentOutOfRangeException(nameof(maxAccelerationCap));

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Category = category;
        MetricType = metricType;
        TargetOperator = targetOperator;
        MeasurementFrequency = measurementFrequency;
        AccelerationType = accelerationType;
        AccelerationAmount = accelerationAmount;
        MaxAccelerationCap = maxAccelerationCap;
        TargetValue = targetValue;
        TargetUnit = string.IsNullOrWhiteSpace(targetUnit) ? null : targetUnit.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(Guid? updatedBy = null)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid? updatedBy = null)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Effective cap: uses MaxAccelerationCap or falls back to 75%.</summary>
    public decimal EffectiveCap => MaxAccelerationCap ?? 75m;

    /// <summary>Reconstitutes from persistence without domain validation. For repositories only.</summary>
    public static MilestoneTemplate Reconstitute(
        Guid id, Guid clientId, Guid companyId,
        string name, string? description,
        MilestoneCategory category, MetricType metricType,
        TargetOperator targetOperator, MeasurementFrequency measurementFrequency,
        bool isActive,
        VestingAccelerationType accelerationType, decimal accelerationAmount,
        decimal? maxAccelerationCap,
        decimal? targetValue, string? targetUnit,
        Guid? createdBy, DateTime createdAt, DateTime updatedAt,
        bool isDeleted, DateTime? deletedAt)
    {
        return new MilestoneTemplate
        {
            Id = id,
            ClientId = clientId,
            CompanyId = companyId,
            Name = name,
            Description = description,
            Category = category,
            MetricType = metricType,
            TargetOperator = targetOperator,
            TargetValue = targetValue,
            TargetUnit = targetUnit,
            MeasurementFrequency = measurementFrequency,
            IsActive = isActive,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            MaxAccelerationCap = maxAccelerationCap,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
