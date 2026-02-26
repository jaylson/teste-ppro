using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IVestingMilestoneService
{
    Task<VestingMilestoneListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize, Guid? vestingPlanId = null, string? status = null);
    Task<IEnumerable<VestingMilestoneResponse>> GetByPlanAsync(Guid clientId, Guid vestingPlanId);
    Task<VestingMilestoneResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<VestingMilestoneResponse> CreateAsync(Guid clientId, Guid companyId, CreateVestingMilestoneRequest request, Guid? userId = null);
    Task<VestingMilestoneResponse> AchieveAsync(Guid id, Guid clientId, AchieveMilestoneRequest request, Guid userId);
    Task<VestingMilestoneResponse> MarkAsFailedAsync(Guid id, Guid clientId, Guid userId);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class VestingMilestoneService : IVestingMilestoneService
{
    private readonly IVestingMilestoneRepository _milestoneRepository;
    private readonly IVestingPlanRepository _vestingPlanRepository;

    public VestingMilestoneService(
        IVestingMilestoneRepository milestoneRepository,
        IVestingPlanRepository vestingPlanRepository)
    {
        _milestoneRepository = milestoneRepository;
        _vestingPlanRepository = vestingPlanRepository;
    }

    public async Task<VestingMilestoneListResponse> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        Guid? vestingPlanId = null, string? status = null)
    {
        var (items, total) = await _milestoneRepository.GetPagedAsync(clientId, companyId, page, pageSize, vestingPlanId, status);
        return new VestingMilestoneListResponse(items.Select(m => MapToResponse(m, null)), total, page, pageSize);
    }

    public async Task<IEnumerable<VestingMilestoneResponse>> GetByPlanAsync(Guid clientId, Guid vestingPlanId)
    {
        var items = await _milestoneRepository.GetByPlanAsync(clientId, vestingPlanId);
        return items.Select(m => MapToResponse(m, null));
    }

    public async Task<VestingMilestoneResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingMilestone", id);
        return MapToResponse(milestone, null);
    }

    public async Task<VestingMilestoneResponse> CreateAsync(
        Guid clientId, Guid companyId, CreateVestingMilestoneRequest request, Guid? userId = null)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(request.VestingPlanId, clientId)
            ?? throw new NotFoundException("VestingPlan", request.VestingPlanId);

        var milestone = VestingMilestone.Create(
            clientId,
            request.VestingPlanId,
            companyId,
            request.Name,
            request.MilestoneType,
            request.AccelerationPercentage,
            request.Description,
            request.TargetValue,
            request.TargetUnit,
            request.IsRequiredForFullVesting,
            request.TargetDate,
            userId);

        await _milestoneRepository.AddAsync(milestone);
        return MapToResponse(milestone, plan.Name);
    }

    public async Task<VestingMilestoneResponse> AchieveAsync(
        Guid id, Guid clientId, AchieveMilestoneRequest request, Guid userId)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingMilestone", id);

        milestone.MarkAsAchieved(userId, request.AchievedDate, request.AchievedValue);
        await _milestoneRepository.UpdateAsync(milestone);
        return MapToResponse(milestone, null);
    }

    public async Task<VestingMilestoneResponse> MarkAsFailedAsync(Guid id, Guid clientId, Guid userId)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingMilestone", id);

        milestone.MarkAsFailed(userId);
        await _milestoneRepository.UpdateAsync(milestone);
        return MapToResponse(milestone, null);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingMilestone", id);

        if (milestone.Status == MilestoneStatus.Achieved)
            throw new DomainException("Milestone já atingido não pode ser excluído.");

        await _milestoneRepository.SoftDeleteAsync(id, clientId, userId);
    }

    private static VestingMilestoneResponse MapToResponse(VestingMilestone m, string? planName) => new()
    {
        Id = m.Id,
        ClientId = m.ClientId,
        VestingPlanId = m.VestingPlanId,
        VestingPlanName = planName ?? string.Empty,
        CompanyId = m.CompanyId,
        Name = m.Name,
        Description = m.Description,
        MilestoneType = m.MilestoneType,
        TargetValue = m.TargetValue,
        TargetUnit = m.TargetUnit,
        AccelerationPercentage = m.AccelerationPercentage,
        IsRequiredForFullVesting = m.IsRequiredForFullVesting,
        Status = m.Status,
        TargetDate = m.TargetDate,
        AchievedDate = m.AchievedDate,
        AchievedValue = m.AchievedValue,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
