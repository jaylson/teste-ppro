using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IVestingAccelerationEngine
{
    Task<AccelerationPreviewResponse> GetPreviewAsync(Guid milestoneId, Guid clientId);
    Task<VestingAccelerationResponse> ApplyAccelerationAsync(Guid milestoneId, Guid clientId, Guid appliedBy);
    Task<IEnumerable<VestingAccelerationResponse>> GetByGrantAsync(Guid clientId, Guid vestingGrantId);
}

public class VestingAccelerationEngine : IVestingAccelerationEngine
{
    private readonly IGrantMilestoneRepository _milestoneRepo;
    private readonly IVestingGrantRepository _grantRepo;
    private readonly IVestingAccelerationRepository _accelerationRepo;
    private readonly IMilestoneTemplateRepository _templateRepo;

    // Default maximum cumulative acceleration (75%) if not set in template
    private const decimal DefaultMaxCap = 75m;

    public VestingAccelerationEngine(
        IGrantMilestoneRepository milestoneRepo,
        IVestingGrantRepository grantRepo,
        IVestingAccelerationRepository accelerationRepo,
        IMilestoneTemplateRepository templateRepo)
    {
        _milestoneRepo = milestoneRepo;
        _grantRepo = grantRepo;
        _accelerationRepo = accelerationRepo;
        _templateRepo = templateRepo;
    }

    /// <summary>
    /// Returns a preview of the acceleration impact without persisting anything.
    /// </summary>
    public async Task<AccelerationPreviewResponse> GetPreviewAsync(Guid milestoneId, Guid clientId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, clientId)
            ?? throw new NotFoundException("GrantMilestone", milestoneId);

        if (!milestone.IsAchieved)
            throw new InvalidOperationException("Apenas milestones atingidos podem ter aceleração calculada.");

        var grant = await _grantRepo.GetByIdAsync(milestone.VestingGrantId, clientId)
            ?? throw new NotFoundException("VestingGrant", milestone.VestingGrantId);

        var currentCumulative = await _accelerationRepo.GetTotalAccelerationForGrantAsync(clientId, grant.Id);
        var effectiveCap = await GetEffectiveCap(milestone, clientId);

        var (newEndDate, sharesAccelerated) = Calculate(milestone, grant);
        var exceeds = (currentCumulative + milestone.AccelerationAmount) > effectiveCap;
        var monthsAccelerated = (int)Math.Round(
            (grant.VestingEndDate - newEndDate).TotalDays / 30.44);

        return new AccelerationPreviewResponse
        {
            GrantMilestoneId = milestone.Id,
            MilestoneName = milestone.Name,
            AccelerationType = milestone.AccelerationType,
            AccelerationAmount = milestone.AccelerationAmount,
            CurrentVestingEndDate = grant.VestingEndDate,
            ProjectedVestingEndDate = newEndDate,
            AdditionalSharesUnlocked = sharesAccelerated,
            MonthsAccelerated = monthsAccelerated,
            CurrentCumulativeAcceleration = currentCumulative,
            EffectiveCap = effectiveCap,
            ExceedsCap = exceeds
        };
    }

    /// <summary>
    /// Applies the acceleration to the vesting grant.
    /// Guards against double-apply (unique constraint in DB) and cap enforcement.
    /// </summary>
    public async Task<VestingAccelerationResponse> ApplyAccelerationAsync(
        Guid milestoneId, Guid clientId, Guid appliedBy)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, clientId)
            ?? throw new NotFoundException("GrantMilestone", milestoneId);

        if (!milestone.CanApplyAcceleration)
            throw new InvalidOperationException(
                $"Aceleração não pode ser aplicada: status={milestone.Status}, " +
                $"verificado={milestone.VerifiedAt.HasValue}, aplicado={milestone.AccelerationApplied}.");

        var grant = await _grantRepo.GetByIdAsync(milestone.VestingGrantId, clientId)
            ?? throw new NotFoundException("VestingGrant", milestone.VestingGrantId);

        // Enforce cap
        var currentCumulative = await _accelerationRepo.GetTotalAccelerationForGrantAsync(clientId, grant.Id);
        var effectiveCap = await GetEffectiveCap(milestone, clientId);
        if (currentCumulative + milestone.AccelerationAmount > effectiveCap)
        {
            throw new InvalidOperationException(
                $"Aceleração excederia o cap máximo de {effectiveCap}%. " +
                $"Acumulado atual: {currentCumulative}%. Tentativa: +{milestone.AccelerationAmount}%.");
        }

        var originalEndDate = grant.VestingEndDate;
        var (newEndDate, sharesAccelerated) = Calculate(milestone, grant);

        // Create acceleration record (unique per milestone enforced by DB)
        var acceleration = VestingAcceleration.Create(
            clientId,
            grant.Id,
            milestoneId,
            milestone.AccelerationType,
            milestone.AccelerationAmount,
            originalEndDate,
            newEndDate,
            sharesAccelerated,
            appliedBy);

        // Apply to grant
        grant.ApplyAcceleration(newEndDate, appliedBy);

        // Mark milestone as applied (uses internal method)
        milestone.MarkAccelerationApplied(appliedBy);

        // Persist all 3 changes
        await _accelerationRepo.AddAsync(acceleration);
        await _grantRepo.UpdateAsync(grant);
        await _milestoneRepo.UpdateAsync(milestone);

        return new VestingAccelerationResponse
        {
            Id = acceleration.Id,
            VestingGrantId = acceleration.VestingGrantId,
            GrantMilestoneId = acceleration.GrantMilestoneId,
            MilestoneName = milestone.Name,
            AccelerationType = acceleration.AccelerationType,
            AccelerationAmount = acceleration.AccelerationAmount,
            OriginalVestingEndDate = acceleration.OriginalVestingEndDate,
            NewVestingEndDate = acceleration.NewVestingEndDate,
            SharesAccelerated = acceleration.SharesAccelerated,
            MonthsAccelerated = acceleration.MonthsAccelerated,
            AppliedAt = acceleration.AppliedAt,
            AppliedBy = acceleration.AppliedBy
        };
    }

    public async Task<IEnumerable<VestingAccelerationResponse>> GetByGrantAsync(
        Guid clientId, Guid vestingGrantId)
    {
        var accelerations = await _accelerationRepo.GetByGrantAsync(clientId, vestingGrantId);
        var milestones = (await _milestoneRepo.GetByGrantAsync(clientId, vestingGrantId)).ToList();

        return accelerations.Select(a =>
        {
            var milestone = milestones.FirstOrDefault(m => m.Id == a.GrantMilestoneId);
            return new VestingAccelerationResponse
            {
                Id = a.Id,
                VestingGrantId = a.VestingGrantId,
                GrantMilestoneId = a.GrantMilestoneId,
                MilestoneName = milestone?.Name ?? string.Empty,
                AccelerationType = a.AccelerationType,
                AccelerationAmount = a.AccelerationAmount,
                OriginalVestingEndDate = a.OriginalVestingEndDate,
                NewVestingEndDate = a.NewVestingEndDate,
                SharesAccelerated = a.SharesAccelerated,
                MonthsAccelerated = a.MonthsAccelerated,
                AppliedAt = a.AppliedAt,
                AppliedBy = a.AppliedBy
            };
        });
    }

    // ─── Calculation Core ────────────────────────────────────────────────────

    /// <summary>
    /// Pure calculation: returns the new vesting end date and shares accelerated.
    /// Does NOT persist anything.
    /// </summary>
    private static (DateTime NewEndDate, decimal SharesAccelerated) Calculate(
        GrantMilestone milestone, VestingGrant grant)
    {
        var currentEndDate = grant.VestingEndDate;
        var totalDays = (currentEndDate - grant.VestingStartDate).TotalDays;

        DateTime newEndDate;
        decimal sharesAccelerated;

        switch (milestone.AccelerationType)
        {
            case VestingAccelerationType.Percentage:
                // Accelerate by X% of the remaining vesting period
                var daysToAccelerate = totalDays * (double)(milestone.AccelerationAmount / 100m);
                newEndDate = currentEndDate.AddDays(-daysToAccelerate);
                // Calculate shares that become available immediately
                var asOfNewEnd = grant.CalculateVestedShares(newEndDate);
                var asOfOldEnd = grant.CalculateVestedShares(currentEndDate);
                sharesAccelerated = Math.Max(asOfNewEnd - asOfOldEnd, 0m);
                break;

            case VestingAccelerationType.Months:
                // Accelerate by a fixed number of months
                newEndDate = currentEndDate.AddMonths(-(int)milestone.AccelerationAmount);
                var sharesAtNew = grant.CalculateVestedShares(newEndDate);
                var sharesAtOld = grant.CalculateVestedShares(currentEndDate);
                sharesAccelerated = Math.Max(sharesAtNew - sharesAtOld, 0m);
                break;

            case VestingAccelerationType.Shares:
                // Immediately unlock a specific count of unvested shares
                var targetShares = milestone.AccelerationAmount;
                var currentVested = grant.VestedShares;
                var remainingUnvested = grant.TotalShares - currentVested;
                sharesAccelerated = Math.Min(targetShares, remainingUnvested);
                // Calculate how many days earlier the vesting ends
                var daysPerShare = totalDays / (double)grant.TotalShares;
                var daysEarlier = daysPerShare * (double)sharesAccelerated;
                newEndDate = currentEndDate.AddDays(-daysEarlier);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(milestone.AccelerationType),
                    $"Tipo de aceleração desconhecido: {milestone.AccelerationType}");
        }

        // Guard: new end date cannot be before vesting start
        if (newEndDate < grant.VestingStartDate)
            newEndDate = grant.VestingStartDate;

        // Guard: new end date cannot be after current end date
        if (newEndDate > currentEndDate)
            newEndDate = currentEndDate;

        return (newEndDate, sharesAccelerated);
    }

    /// <summary>
    /// Returns the effective acceleration cap for a milestone:
    /// uses the template cap if the milestone was created from a template, otherwise the default.
    /// </summary>
    private async Task<decimal> GetEffectiveCap(GrantMilestone milestone, Guid clientId)
    {
        if (milestone.MilestoneTemplateId.HasValue)
        {
            var template = await _templateRepo.GetByIdAsync(milestone.MilestoneTemplateId.Value, clientId);
            if (template is not null)
                return template.EffectiveCap;
        }

        return DefaultMaxCap;
    }
}
