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
