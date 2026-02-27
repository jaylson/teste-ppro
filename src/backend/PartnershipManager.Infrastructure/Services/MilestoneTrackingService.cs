using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IMilestoneTrackingService
{
    Task<GrantMilestoneListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize, Guid? vestingGrantId = null, string? status = null, string? category = null);
    Task<IEnumerable<GrantMilestoneResponse>> GetByGrantAsync(Guid clientId, Guid vestingGrantId);
    Task<GrantMilestoneResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<GrantMilestoneResponse> CreateAsync(Guid clientId, Guid companyId, CreateGrantMilestoneRequest request, Guid? userId = null);
    Task<GrantMilestoneResponse> RecordProgressAsync(Guid id, Guid clientId, RecordMilestoneProgressRequest request, Guid userId);
    Task<GrantMilestoneResponse> MarkAsAchievedAsync(Guid id, Guid clientId, AchieveGrantMilestoneRequest request, Guid userId);
    Task<GrantMilestoneResponse> VerifyAsync(Guid id, Guid clientId, Guid verifiedBy);
    Task<GrantMilestoneResponse> MarkAsFailedAsync(Guid id, Guid clientId, Guid userId);
    Task<GrantMilestoneResponse> CancelAsync(Guid id, Guid clientId, Guid userId);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
    Task<IEnumerable<MilestoneProgressResponse>> GetProgressHistoryAsync(Guid clientId, Guid milestoneId);
    Task<IEnumerable<MilestoneProgressResponse>> GetProgressTimeSeriesAsync(Guid clientId, Guid milestoneId, DateTime from, DateTime to);
    Task<MilestoneProgressDashboardResponse> GetDashboardAsync(Guid clientId, Guid vestingGrantId);
}

public class MilestoneTrackingService : IMilestoneTrackingService
{
    private readonly IGrantMilestoneRepository _milestoneRepo;
    private readonly IMilestoneProgressRepository _progressRepo;
    private readonly IVestingAccelerationRepository _accelerationRepo;
    private readonly IVestingGrantRepository _grantRepo;

    public MilestoneTrackingService(
        IGrantMilestoneRepository milestoneRepo,
        IMilestoneProgressRepository progressRepo,
        IVestingAccelerationRepository accelerationRepo,
        IVestingGrantRepository grantRepo)
    {
        _milestoneRepo = milestoneRepo;
        _progressRepo = progressRepo;
        _accelerationRepo = accelerationRepo;
        _grantRepo = grantRepo;
    }

    public async Task<GrantMilestoneListResponse> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        Guid? vestingGrantId = null, string? status = null, string? category = null)
    {
        var (items, total) = await _milestoneRepo.GetPagedAsync(clientId, companyId, page, pageSize, vestingGrantId, status, category);
        return new GrantMilestoneListResponse(items.Select(MapMilestone), total, page, pageSize);
    }

    public async Task<IEnumerable<GrantMilestoneResponse>> GetByGrantAsync(Guid clientId, Guid vestingGrantId)
    {
        var items = await _milestoneRepo.GetByGrantAsync(clientId, vestingGrantId);
        return items.Select(MapMilestone);
    }

    public async Task<GrantMilestoneResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);
        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> CreateAsync(
        Guid clientId, Guid companyId, CreateGrantMilestoneRequest request, Guid? userId = null)
    {
        var grant = await _grantRepo.GetByIdAsync(request.VestingGrantId, clientId)
            ?? throw new NotFoundException("VestingGrant", request.VestingGrantId);

        var milestone = GrantMilestone.Create(
            clientId,
            request.VestingGrantId,
            companyId,
            request.Name,
            request.Category,
            request.MetricType,
            request.TargetValue,
            request.TargetOperator,
            request.TargetDate,
            request.MeasurementFrequency,
            request.AccelerationType,
            request.AccelerationAmount,
            request.Description,
            request.MilestoneTemplateId,
            userId);

        await _milestoneRepo.AddAsync(milestone);
        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> RecordProgressAsync(
        Guid id, Guid clientId, RecordMilestoneProgressRequest request, Guid userId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);

        var progressPct = milestone.RecordProgress(request.RecordedValue, userId);

        var progress = MilestoneProgress.Create(
            clientId,
            id,
            request.RecordedDate,
            request.RecordedValue,
            progressPct,
            userId,
            request.Notes,
            request.DataSource);

        await _milestoneRepo.UpdateAsync(milestone);
        await _progressRepo.AddAsync(progress);

        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> MarkAsAchievedAsync(
        Guid id, Guid clientId, AchieveGrantMilestoneRequest request, Guid userId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);

        milestone.MarkAsAchieved(request.AchievedValue, userId);
        await _milestoneRepo.UpdateAsync(milestone);
        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> VerifyAsync(Guid id, Guid clientId, Guid verifiedBy)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);

        milestone.Verify(verifiedBy);
        await _milestoneRepo.UpdateAsync(milestone);
        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> MarkAsFailedAsync(Guid id, Guid clientId, Guid userId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);

        milestone.MarkAsFailed(userId);
        await _milestoneRepo.UpdateAsync(milestone);
        return MapMilestone(milestone);
    }

    public async Task<GrantMilestoneResponse> CancelAsync(Guid id, Guid clientId, Guid userId)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);

        milestone.Cancel(userId);
        await _milestoneRepo.UpdateAsync(milestone);
        return MapMilestone(milestone);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        _ = await _milestoneRepo.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("GrantMilestone", id);
        await _milestoneRepo.SoftDeleteAsync(id, clientId, userId);
    }

    public async Task<IEnumerable<MilestoneProgressResponse>> GetProgressHistoryAsync(
        Guid clientId, Guid milestoneId)
    {
        var items = await _progressRepo.GetByMilestoneAsync(clientId, milestoneId);
        return items.Select(MapProgress);
    }

    public async Task<IEnumerable<MilestoneProgressResponse>> GetProgressTimeSeriesAsync(
        Guid clientId, Guid milestoneId, DateTime from, DateTime to)
    {
        var items = await _progressRepo.GetTimeSeriesAsync(clientId, milestoneId, from, to);
        return items.Select(MapProgress);
    }

    public async Task<MilestoneProgressDashboardResponse> GetDashboardAsync(
        Guid clientId, Guid vestingGrantId)
    {
        var milestones = (await _milestoneRepo.GetByGrantAsync(clientId, vestingGrantId)).ToList();
        var accelerations = (await _accelerationRepo.GetByGrantAsync(clientId, vestingGrantId)).ToList();

        return new MilestoneProgressDashboardResponse
        {
            VestingGrantId = vestingGrantId,
            TotalMilestones = milestones.Count,
            PendingMilestones = milestones.Count(m => m.Status == MilestoneStatus.Pending),
            InProgressMilestones = milestones.Count(m => m.Status == MilestoneStatus.InProgress),
            AchievedMilestones = milestones.Count(m => m.Status == MilestoneStatus.Achieved),
            FailedMilestones = milestones.Count(m => m.Status == MilestoneStatus.Failed),
            TotalAppliedAcceleration = milestones.Where(m => m.AccelerationApplied).Sum(m => m.AccelerationAmount),
            PendingAcceleration = milestones
                .Where(m => m.IsAchieved && m.VerifiedAt.HasValue && !m.AccelerationApplied)
                .Sum(m => m.AccelerationAmount),
            Milestones = milestones.Select(MapMilestone).ToList(),
            AppliedAccelerations = accelerations.Select(a => MapAcceleration(a, milestones)).ToList()
        };
    }

    // ─── Mapping helpers ─────────────────────────────────────────────────────

    internal static GrantMilestoneResponse MapMilestone(GrantMilestone m) =>
        new()
        {
            Id = m.Id,
            ClientId = m.ClientId,
            VestingGrantId = m.VestingGrantId,
            MilestoneTemplateId = m.MilestoneTemplateId,
            CompanyId = m.CompanyId,
            Name = m.Name,
            Description = m.Description,
            Category = m.Category,
            MetricType = m.MetricType,
            TargetValue = m.TargetValue,
            TargetOperator = m.TargetOperator,
            TargetDate = m.TargetDate,
            MeasurementFrequency = m.MeasurementFrequency,
            Status = m.Status,
            CurrentValue = m.CurrentValue,
            ProgressPercentage = m.ProgressPercentage,
            AchievedAt = m.AchievedAt,
            AchievedValue = m.AchievedValue,
            VerifiedAt = m.VerifiedAt,
            VerifiedBy = m.VerifiedBy,
            AccelerationType = m.AccelerationType,
            AccelerationAmount = m.AccelerationAmount,
            AccelerationApplied = m.AccelerationApplied,
            AccelerationAppliedAt = m.AccelerationAppliedAt,
            CanApplyAcceleration = m.CanApplyAcceleration,
            IsExpired = m.IsExpired,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };

    private static MilestoneProgressResponse MapProgress(MilestoneProgress p) =>
        new()
        {
            Id = p.Id,
            GrantMilestoneId = p.GrantMilestoneId,
            RecordedDate = p.RecordedDate,
            RecordedValue = p.RecordedValue,
            ProgressPercentage = p.ProgressPercentage,
            Notes = p.Notes,
            DataSource = p.DataSource,
            RecordedBy = p.RecordedBy,
            CreatedAt = p.CreatedAt
        };

    private static VestingAccelerationResponse MapAcceleration(
        VestingAcceleration a, IList<GrantMilestone> milestones)
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
    }
}
