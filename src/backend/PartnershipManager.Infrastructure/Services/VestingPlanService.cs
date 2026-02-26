using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IVestingPlanService
{
    Task<VestingPlanListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize, string? search = null, string? status = null);
    Task<IEnumerable<VestingPlanResponse>> GetByCompanyAsync(Guid clientId, Guid companyId, string? status = null);
    Task<VestingPlanResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<VestingPlanResponse> CreateAsync(Guid clientId, CreateVestingPlanRequest request, Guid? userId = null);
    Task<VestingPlanResponse> UpdateAsync(Guid id, Guid clientId, UpdateVestingPlanRequest request, Guid userId);
    Task<VestingPlanResponse> ActivateAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingPlanResponse> DeactivateAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingPlanResponse> ArchiveAsync(Guid id, Guid clientId, Guid userId);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class VestingPlanService : IVestingPlanService
{
    private readonly IVestingPlanRepository _vestingPlanRepository;
    private readonly ICompanyRepository _companyRepository;

    public VestingPlanService(
        IVestingPlanRepository vestingPlanRepository,
        ICompanyRepository companyRepository)
    {
        _vestingPlanRepository = vestingPlanRepository;
        _companyRepository = companyRepository;
    }

    public async Task<VestingPlanListResponse> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? search = null, string? status = null)
    {
        var (items, total) = await _vestingPlanRepository.GetPagedAsync(clientId, companyId, page, pageSize, search, status);
        return new VestingPlanListResponse(items.Select(MapToResponse), total, page, pageSize);
    }

    public async Task<IEnumerable<VestingPlanResponse>> GetByCompanyAsync(Guid clientId, Guid companyId, string? status = null)
    {
        var items = await _vestingPlanRepository.GetByCompanyAsync(clientId, companyId, status);
        return items.Select(MapToResponse);
    }

    public async Task<VestingPlanResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingPlan", id);
        return MapToResponse(plan);
    }

    public async Task<VestingPlanResponse> CreateAsync(Guid clientId, CreateVestingPlanRequest request, Guid? userId = null)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);
        if (company == null || company.ClientId != clientId)
            throw new NotFoundException("Company", request.CompanyId);

        if (await _vestingPlanRepository.NameExistsAsync(clientId, request.CompanyId, request.Name))
            throw new DomainException($"Já existe um plano de vesting com o nome '{request.Name}' nesta empresa.");

        var plan = VestingPlan.Create(
            clientId,
            request.CompanyId,
            request.Name,
            request.VestingType,
            request.CliffMonths,
            request.VestingMonths,
            request.TotalEquityPercentage,
            request.Description,
            userId);

        await _vestingPlanRepository.AddAsync(plan);
        return MapToResponse(plan);
    }

    public async Task<VestingPlanResponse> UpdateAsync(Guid id, Guid clientId, UpdateVestingPlanRequest request, Guid userId)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingPlan", id);

        if (await _vestingPlanRepository.NameExistsAsync(clientId, plan.CompanyId, request.Name, id))
            throw new DomainException($"Já existe um plano de vesting com o nome '{request.Name}' nesta empresa.");

        plan.UpdateDetails(
            request.Name,
            request.Description,
            request.CliffMonths,
            request.VestingMonths,
            request.TotalEquityPercentage,
            userId);

        await _vestingPlanRepository.UpdateAsync(plan);
        return MapToResponse(plan);
    }

    public async Task<VestingPlanResponse> ActivateAsync(Guid id, Guid clientId, Guid userId)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingPlan", id);

        plan.Activate(userId);
        await _vestingPlanRepository.UpdateAsync(plan);
        return MapToResponse(plan);
    }

    public async Task<VestingPlanResponse> DeactivateAsync(Guid id, Guid clientId, Guid userId)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingPlan", id);

        plan.Deactivate(userId);
        await _vestingPlanRepository.UpdateAsync(plan);
        return MapToResponse(plan);
    }

    public async Task<VestingPlanResponse> ArchiveAsync(Guid id, Guid clientId, Guid userId)
    {
        var plan = await _vestingPlanRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingPlan", id);

        plan.Archive(userId);
        await _vestingPlanRepository.UpdateAsync(plan);
        return MapToResponse(plan);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        if (!await _vestingPlanRepository.ExistsAsync(id, clientId))
            throw new NotFoundException("VestingPlan", id);

        await _vestingPlanRepository.SoftDeleteAsync(id, clientId, userId);
    }

    private static VestingPlanResponse MapToResponse(VestingPlan p) => new()
    {
        Id = p.Id,
        ClientId = p.ClientId,
        CompanyId = p.CompanyId,
        Name = p.Name,
        Description = p.Description,
        VestingType = p.VestingType,
        CliffMonths = p.CliffMonths,
        VestingMonths = p.VestingMonths,
        TotalEquityPercentage = p.TotalEquityPercentage,
        Status = p.Status,
        ActivatedAt = p.ActivatedAt,
        ActivatedBy = p.ActivatedBy,
        ActiveGrantsCount = 0, // populated separately if needed
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
