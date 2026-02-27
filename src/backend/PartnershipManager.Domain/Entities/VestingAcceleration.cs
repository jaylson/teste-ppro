using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Immutable ledger entry recording a vesting acceleration applied to a grant
/// when a performance milestone was achieved and approved.
/// One acceleration per milestone (enforced by unique key in DB).
/// </summary>
public class VestingAcceleration
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid VestingGrantId { get; private set; }
    public Guid GrantMilestoneId { get; private set; }

    public VestingAccelerationType AccelerationType { get; private set; }
    public decimal AccelerationAmount { get; private set; }

    // Before / after snapshot
    public DateTime OriginalVestingEndDate { get; private set; }
    public DateTime NewVestingEndDate { get; private set; }
    public decimal SharesAccelerated { get; private set; }

    public DateTime AppliedAt { get; private set; }
    public Guid AppliedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public virtual VestingGrant? Grant { get; private set; }
    public virtual GrantMilestone? Milestone { get; private set; }

    // Derived helpers
    public int MonthsAccelerated =>
        (int)Math.Round((OriginalVestingEndDate - NewVestingEndDate).TotalDays / 30.44);

    private VestingAcceleration() { }

    public static VestingAcceleration Create(
        Guid clientId,
        Guid vestingGrantId,
        Guid grantMilestoneId,
        VestingAccelerationType accelerationType,
        decimal accelerationAmount,
        DateTime originalVestingEndDate,
        DateTime newVestingEndDate,
        decimal sharesAccelerated,
        Guid appliedBy)
    {
        if (accelerationAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(accelerationAmount), "AccelerationAmount deve ser positivo.");
        if (sharesAccelerated < 0)
            throw new ArgumentOutOfRangeException(nameof(sharesAccelerated), "SharesAccelerated não pode ser negativo.");
        if (newVestingEndDate > originalVestingEndDate)
            throw new ArgumentException("NewVestingEndDate não pode ser posterior ao OriginalVestingEndDate.", nameof(newVestingEndDate));

        return new VestingAcceleration
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            GrantMilestoneId = grantMilestoneId,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            OriginalVestingEndDate = originalVestingEndDate.Date,
            NewVestingEndDate = newVestingEndDate.Date,
            SharesAccelerated = sharesAccelerated,
            AppliedAt = DateTime.UtcNow,
            AppliedBy = appliedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Reconstitutes from persistence. For repositories only.</summary>
    public static VestingAcceleration Reconstitute(
        Guid id, Guid clientId, Guid vestingGrantId, Guid grantMilestoneId,
        VestingAccelerationType accelerationType, decimal accelerationAmount,
        DateTime originalVestingEndDate, DateTime newVestingEndDate, decimal sharesAccelerated,
        DateTime appliedAt, Guid appliedBy, DateTime createdAt)
    {
        return new VestingAcceleration
        {
            Id = id,
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            GrantMilestoneId = grantMilestoneId,
            AccelerationType = accelerationType,
            AccelerationAmount = accelerationAmount,
            OriginalVestingEndDate = originalVestingEndDate,
            NewVestingEndDate = newVestingEndDate,
            SharesAccelerated = sharesAccelerated,
            AppliedAt = appliedAt,
            AppliedBy = appliedBy,
            CreatedAt = createdAt
        };
    }
}
