using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Vesting.DTOs;

// ─── Vesting Plan DTOs ────────────────────────────────────────────────────────

public record VestingPlanResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public VestingType VestingType { get; init; }
    public int CliffMonths { get; init; }
    public int VestingMonths { get; init; }
    public decimal TotalEquityPercentage { get; init; }
    public VestingPlanStatus Status { get; init; }
    public DateTime? ActivatedAt { get; init; }
    public Guid? ActivatedBy { get; init; }
    public int ActiveGrantsCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class VestingPlanListResponse : PagedResult<VestingPlanResponse>
{
    public VestingPlanListResponse(IEnumerable<VestingPlanResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateVestingPlanRequest
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public VestingType VestingType { get; init; }
    public int CliffMonths { get; init; }
    public int VestingMonths { get; init; }
    public decimal TotalEquityPercentage { get; init; }
}

public record UpdateVestingPlanRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CliffMonths { get; init; }
    public int VestingMonths { get; init; }
    public decimal TotalEquityPercentage { get; init; }
}

// ─── Vesting Grant DTOs ───────────────────────────────────────────────────────

public record VestingGrantResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid VestingPlanId { get; init; }
    public string VestingPlanName { get; init; } = string.Empty;
    public Guid ShareholderId { get; init; }
    public string ShareholderName { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }

    public DateTime GrantDate { get; init; }
    public decimal TotalShares { get; init; }
    public decimal SharePrice { get; init; }
    public decimal EquityPercentage { get; init; }

    public DateTime VestingStartDate { get; init; }
    public DateTime VestingEndDate { get; init; }
    public DateTime? CliffDate { get; init; }

    public VestingGrantDetailStatus Status { get; init; }
    public decimal VestedShares { get; init; }
    public decimal ExercisedShares { get; init; }
    public decimal AvailableToExercise { get; init; }
    public decimal VestedPercentage { get; init; }

    public DateTime? ApprovedAt { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class VestingGrantListResponse : PagedResult<VestingGrantResponse>
{
    public VestingGrantListResponse(IEnumerable<VestingGrantResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateVestingGrantRequest
{
    public Guid VestingPlanId { get; init; }
    public Guid ShareholderId { get; init; }
    public DateTime GrantDate { get; init; }
    public decimal TotalShares { get; init; }
    public decimal SharePrice { get; init; }
    public decimal EquityPercentage { get; init; }
    public DateTime VestingStartDate { get; init; }
    public string? Notes { get; init; }
}

public record ExerciseSharesRequest
{
    public decimal SharesToExercise { get; init; }
    public decimal SharePriceAtExercise { get; init; }
    public DateTime ExerciseDate { get; init; }
    public VestingTransactionType TransactionType { get; init; } = VestingTransactionType.Exercise;
    public string? Notes { get; init; }
}

// ─── Milestone DTOs ───────────────────────────────────────────────────────────

public record VestingMilestoneResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid VestingPlanId { get; init; }
    public string VestingPlanName { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneType MilestoneType { get; init; }
    public decimal? TargetValue { get; init; }
    public string? TargetUnit { get; init; }
    public decimal AccelerationPercentage { get; init; }
    public bool IsRequiredForFullVesting { get; init; }
    public MilestoneStatus Status { get; init; }
    public DateTime? TargetDate { get; init; }
    public DateTime? AchievedDate { get; init; }
    public decimal? AchievedValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class VestingMilestoneListResponse : PagedResult<VestingMilestoneResponse>
{
    public VestingMilestoneListResponse(IEnumerable<VestingMilestoneResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateVestingMilestoneRequest
{
    public Guid VestingPlanId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneType MilestoneType { get; init; }
    public decimal? TargetValue { get; init; }
    public string? TargetUnit { get; init; }
    public decimal AccelerationPercentage { get; init; }
    public bool IsRequiredForFullVesting { get; init; }
    public DateTime? TargetDate { get; init; }
}

public record AchieveMilestoneRequest
{
    public DateTime AchievedDate { get; init; }
    public decimal? AchievedValue { get; init; }
}

// ─── Calculation / Projection DTOs ───────────────────────────────────────────

public record VestingCalculationResult
{
    public Guid GrantId { get; init; }
    public DateTime AsOfDate { get; init; }
    public decimal TotalShares { get; init; }
    public decimal VestedShares { get; init; }
    public decimal ExercisedShares { get; init; }
    public decimal AvailableToExercise { get; init; }
    public decimal UnvestedShares { get; init; }
    public decimal VestedPercentage { get; init; }
    public bool IsCliffMet { get; init; }
    public bool IsFullyVested { get; init; }
}

public record VestingProjectionPoint
{
    public DateTime Date { get; init; }
    public decimal VestedShares { get; init; }
    public decimal VestedPercentage { get; init; }
}

public record VestingProjectionResponse
{
    public Guid GrantId { get; init; }
    public DateTime ProjectionEndDate { get; init; }
    public IReadOnlyList<VestingProjectionPoint> Points { get; init; } = [];
}

// ─── Transaction DTOs ─────────────────────────────────────────────────────────

public record VestingTransactionResponse
{
    public Guid Id { get; init; }
    public Guid VestingGrantId { get; init; }
    public Guid ShareholderId { get; init; }
    public string ShareholderName { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public decimal SharesExercised { get; init; }
    public decimal SharePriceAtExercise { get; init; }
    public decimal StrikePrice { get; init; }
    public decimal TotalExerciseValue { get; init; }
    public decimal GainAmount { get; init; }
    public VestingTransactionType TransactionType { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class VestingTransactionListResponse : PagedResult<VestingTransactionResponse>
{
    public VestingTransactionListResponse(IEnumerable<VestingTransactionResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

// ─── Milestone Template DTOs ──────────────────────────────────────────────────

public record MilestoneTemplateResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneCategory Category { get; init; }
    public MetricType MetricType { get; init; }
    public TargetOperator TargetOperator { get; init; }
    public MeasurementFrequency MeasurementFrequency { get; init; }
    public bool IsActive { get; init; }
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public decimal? MaxAccelerationCap { get; init; }
    public decimal EffectiveCap { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class MilestoneTemplateListResponse : PagedResult<MilestoneTemplateResponse>
{
    public MilestoneTemplateListResponse(IEnumerable<MilestoneTemplateResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateMilestoneTemplateRequest
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneCategory Category { get; init; }
    public MetricType MetricType { get; init; }
    public TargetOperator TargetOperator { get; init; }
    public MeasurementFrequency MeasurementFrequency { get; init; }
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public decimal? MaxAccelerationCap { get; init; }
}

public record UpdateMilestoneTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneCategory Category { get; init; }
    public MetricType MetricType { get; init; }
    public TargetOperator TargetOperator { get; init; }
    public MeasurementFrequency MeasurementFrequency { get; init; }
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public decimal? MaxAccelerationCap { get; init; }
}

// ─── Grant Milestone DTOs ─────────────────────────────────────────────────────

public record GrantMilestoneResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid VestingGrantId { get; init; }
    public Guid? MilestoneTemplateId { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneCategory Category { get; init; }
    public MetricType MetricType { get; init; }
    public decimal TargetValue { get; init; }
    public TargetOperator TargetOperator { get; init; }
    public DateTime TargetDate { get; init; }
    public MeasurementFrequency MeasurementFrequency { get; init; }
    public MilestoneStatus Status { get; init; }
    public decimal? CurrentValue { get; init; }
    public decimal ProgressPercentage { get; init; }
    public DateTime? AchievedAt { get; init; }
    public decimal? AchievedValue { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public Guid? VerifiedBy { get; init; }
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public bool AccelerationApplied { get; init; }
    public DateTime? AccelerationAppliedAt { get; init; }
    public bool CanApplyAcceleration { get; init; }
    public bool IsExpired { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class GrantMilestoneListResponse : PagedResult<GrantMilestoneResponse>
{
    public GrantMilestoneListResponse(IEnumerable<GrantMilestoneResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize) { }
}

public record CreateGrantMilestoneRequest
{
    public Guid VestingGrantId { get; init; }
    public Guid? MilestoneTemplateId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MilestoneCategory Category { get; init; }
    public MetricType MetricType { get; init; }
    public decimal TargetValue { get; init; }
    public TargetOperator TargetOperator { get; init; }
    public DateTime TargetDate { get; init; }
    public MeasurementFrequency MeasurementFrequency { get; init; }
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
}

public record VerifyGrantMilestoneRequest
{
    /// <summary>Optional note from the approver.</summary>
    public string? Notes { get; init; }
}

public record AchieveGrantMilestoneRequest
{
    public decimal AchievedValue { get; init; }
}

// ─── Milestone Progress DTOs ──────────────────────────────────────────────────

public record MilestoneProgressResponse
{
    public Guid Id { get; init; }
    public Guid GrantMilestoneId { get; init; }
    public DateTime RecordedDate { get; init; }
    public decimal RecordedValue { get; init; }
    public decimal ProgressPercentage { get; init; }
    public string? Notes { get; init; }
    public ProgressDataSource? DataSource { get; init; }
    public Guid RecordedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record RecordMilestoneProgressRequest
{
    public DateTime RecordedDate { get; init; }
    public decimal RecordedValue { get; init; }
    public string? Notes { get; init; }
    public ProgressDataSource DataSource { get; init; } = ProgressDataSource.Manual;
}

// ─── Vesting Acceleration DTOs ────────────────────────────────────────────────

public record VestingAccelerationResponse
{
    public Guid Id { get; init; }
    public Guid VestingGrantId { get; init; }
    public Guid GrantMilestoneId { get; init; }
    public string MilestoneName { get; init; } = string.Empty;
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public DateTime OriginalVestingEndDate { get; init; }
    public DateTime NewVestingEndDate { get; init; }
    public decimal SharesAccelerated { get; init; }
    public int MonthsAccelerated { get; init; }
    public DateTime AppliedAt { get; init; }
    public Guid AppliedBy { get; init; }
}

public record AccelerationPreviewResponse
{
    public Guid GrantMilestoneId { get; init; }
    public string MilestoneName { get; init; } = string.Empty;
    public VestingAccelerationType AccelerationType { get; init; }
    public decimal AccelerationAmount { get; init; }
    public DateTime CurrentVestingEndDate { get; init; }
    public DateTime ProjectedVestingEndDate { get; init; }
    public decimal AdditionalSharesUnlocked { get; init; }
    public int MonthsAccelerated { get; init; }
    public decimal CurrentCumulativeAcceleration { get; init; }
    public decimal EffectiveCap { get; init; }
    public bool ExceedsCap { get; init; }
}

// ─── Dashboard DTOs ───────────────────────────────────────────────────────────

public record MilestoneProgressDashboardResponse
{
    public Guid VestingGrantId { get; init; }
    public int TotalMilestones { get; init; }
    public int PendingMilestones { get; init; }
    public int InProgressMilestones { get; init; }
    public int AchievedMilestones { get; init; }
    public int FailedMilestones { get; init; }
    public decimal TotalAppliedAcceleration { get; init; }
    public decimal PendingAcceleration { get; init; }
    public IReadOnlyList<GrantMilestoneResponse> Milestones { get; init; } = [];
    public IReadOnlyList<VestingAccelerationResponse> AppliedAccelerations { get; init; } = [];
}
