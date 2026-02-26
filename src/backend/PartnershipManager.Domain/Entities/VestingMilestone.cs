using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

public class VestingMilestone : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid VestingPlanId { get; private set; }
    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MilestoneType MilestoneType { get; private set; }
    public decimal? TargetValue { get; private set; }
    public string? TargetUnit { get; private set; }

    public decimal AccelerationPercentage { get; private set; }
    public bool IsRequiredForFullVesting { get; private set; }

    public MilestoneStatus Status { get; private set; }
    public DateTime? TargetDate { get; private set; }
    public DateTime? AchievedDate { get; private set; }
    public Guid? AchievedBy { get; private set; }
    public decimal? AchievedValue { get; private set; }

    private VestingMilestone() { }

    public static VestingMilestone Create(
        Guid clientId,
        Guid vestingPlanId,
        Guid companyId,
        string name,
        MilestoneType milestoneType,
        decimal accelerationPercentage,
        string? description = null,
        decimal? targetValue = null,
        string? targetUnit = null,
        bool isRequiredForFullVesting = false,
        DateTime? targetDate = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do milestone é obrigatório.", nameof(name));
        if (accelerationPercentage < 0 || accelerationPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(accelerationPercentage), "Aceleração deve ser entre 0 e 100%.");

        return new VestingMilestone
        {
            ClientId = clientId,
            VestingPlanId = vestingPlanId,
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            MilestoneType = milestoneType,
            TargetValue = targetValue,
            TargetUnit = string.IsNullOrWhiteSpace(targetUnit) ? null : targetUnit.Trim(),
            AccelerationPercentage = accelerationPercentage,
            IsRequiredForFullVesting = isRequiredForFullVesting,
            Status = MilestoneStatus.Pending,
            TargetDate = targetDate.HasValue ? targetDate.Value.Date : null,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    /// <summary>
    /// Marks the milestone as achieved and records when and by whom.
    /// </summary>
    public void MarkAsAchieved(Guid userId, DateTime achievedDate, decimal? achievedValue = null)
    {
        if (Status != MilestoneStatus.Pending)
            throw new InvalidOperationException($"Milestone no status '{Status}' não pode ser marcado como atingido.");

        Status = MilestoneStatus.Achieved;
        AchievedDate = achievedDate.Date;
        AchievedBy = userId;
        AchievedValue = achievedValue;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(Guid userId)
    {
        if (Status != MilestoneStatus.Pending)
            throw new InvalidOperationException($"Milestone no status '{Status}' não pode ser marcado como falho.");

        Status = MilestoneStatus.Failed;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid userId)
    {
        if (Status == MilestoneStatus.Achieved)
            throw new InvalidOperationException("Milestone já atingido não pode ser cancelado.");

        Status = MilestoneStatus.Cancelled;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reconstitutes a VestingMilestone from persistence without domain validation.
    /// </summary>
    public static VestingMilestone Reconstitute(
        Guid id,
        Guid clientId,
        Guid vestingPlanId,
        Guid companyId,
        string name,
        string? description,
        MilestoneType milestoneType,
        decimal? targetValue,
        string? targetUnit,
        decimal accelerationPercentage,
        bool isRequiredForFullVesting,
        MilestoneStatus status,
        DateTime? targetDate,
        DateTime? achievedDate,
        Guid? achievedBy,
        decimal? achievedValue,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new VestingMilestone
        {
            Id = id,
            ClientId = clientId,
            VestingPlanId = vestingPlanId,
            CompanyId = companyId,
            Name = name,
            Description = description,
            MilestoneType = milestoneType,
            TargetValue = targetValue,
            TargetUnit = targetUnit,
            AccelerationPercentage = accelerationPercentage,
            IsRequiredForFullVesting = isRequiredForFullVesting,
            Status = status,
            TargetDate = targetDate,
            AchievedDate = achievedDate,
            AchievedBy = achievedBy,
            AchievedValue = achievedValue,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
