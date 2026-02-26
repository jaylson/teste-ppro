using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

public class VestingSchedule : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid VestingGrantId { get; private set; }
    public Guid CompanyId { get; private set; }

    public int PeriodNumber { get; private set; }
    public DateTime ScheduleDate { get; private set; }
    public decimal SharesToVest { get; private set; }
    public decimal CumulativeShares { get; private set; }
    public decimal PercentageToVest { get; private set; }

    public VestingScheduleStatus Status { get; private set; }
    public DateTime? VestedAt { get; private set; }

    private VestingSchedule() { }

    public static VestingSchedule Create(
        Guid clientId,
        Guid vestingGrantId,
        Guid companyId,
        int periodNumber,
        DateTime scheduleDate,
        decimal sharesToVest,
        decimal cumulativeShares,
        decimal percentageToVest,
        Guid? createdBy = null)
    {
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Período deve ser maior que 0.");
        if (sharesToVest <= 0)
            throw new ArgumentOutOfRangeException(nameof(sharesToVest), "Ações a vestir devem ser positivas.");

        return new VestingSchedule
        {
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            CompanyId = companyId,
            PeriodNumber = periodNumber,
            ScheduleDate = scheduleDate.Date,
            SharesToVest = sharesToVest,
            CumulativeShares = cumulativeShares,
            PercentageToVest = percentageToVest,
            Status = VestingScheduleStatus.Pending,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void MarkAsVested(Guid userId)
    {
        if (Status != VestingScheduleStatus.Pending)
            throw new InvalidOperationException("Apenas períodos pendentes podem ser marcados como vestidos.");

        Status = VestingScheduleStatus.Vested;
        VestedAt = DateTime.UtcNow;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Skip(Guid userId)
    {
        if (Status != VestingScheduleStatus.Pending)
            throw new InvalidOperationException("Apenas períodos pendentes podem ser ignorados.");

        Status = VestingScheduleStatus.Skipped;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reconstitutes a VestingSchedule from persistence without domain validation.
    /// </summary>
    public static VestingSchedule Reconstitute(
        Guid id,
        Guid clientId,
        Guid vestingGrantId,
        Guid companyId,
        int periodNumber,
        DateTime scheduleDate,
        decimal sharesToVest,
        decimal cumulativeShares,
        decimal percentageToVest,
        VestingScheduleStatus status,
        DateTime? vestedAt,
        DateTime createdAt,
        DateTime updatedAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new VestingSchedule
        {
            Id = id,
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            CompanyId = companyId,
            PeriodNumber = periodNumber,
            ScheduleDate = scheduleDate,
            SharesToVest = sharesToVest,
            CumulativeShares = cumulativeShares,
            PercentageToVest = percentageToVest,
            Status = status,
            VestedAt = vestedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
