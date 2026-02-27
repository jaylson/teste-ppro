using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IMilestoneTemplateService
{
    Task<MilestoneTemplateListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize, string? category = null, bool? isActive = null);
    Task<IEnumerable<MilestoneTemplateResponse>> GetByCompanyAsync(Guid clientId, Guid companyId, bool activeOnly = true);
    Task<MilestoneTemplateResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<MilestoneTemplateResponse> CreateAsync(Guid clientId, CreateMilestoneTemplateRequest request, Guid? userId = null);
    Task<MilestoneTemplateResponse> UpdateAsync(Guid id, Guid clientId, UpdateMilestoneTemplateRequest request, Guid? userId = null);
    Task ActivateAsync(Guid id, Guid clientId, Guid? userId = null);
    Task DeactivateAsync(Guid id, Guid clientId, Guid? userId = null);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class MilestoneTemplateService : IMilestoneTemplateService
{
    private readonly IMilestoneTemplateRepository _templateRepository;

    public MilestoneTemplateService(IMilestoneTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<MilestoneTemplateListResponse> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? category = null, bool? isActive = null)
    {
        var (items, total) = await _templateRepository.GetPagedAsync(clientId, companyId, page, pageSize, category, isActive);
        return new MilestoneTemplateListResponse(items.Select(Map), total, page, pageSize);
    }

    public async Task<IEnumerable<MilestoneTemplateResponse>> GetByCompanyAsync(
        Guid clientId, Guid companyId, bool activeOnly = true)
    {
        var items = await _templateRepository.GetByCompanyAsync(clientId, companyId, activeOnly);
        return items.Select(Map);
    }

    public async Task<MilestoneTemplateResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("MilestoneTemplate", id);
        return Map(template);
    }

    public async Task<MilestoneTemplateResponse> CreateAsync(
        Guid clientId, CreateMilestoneTemplateRequest request, Guid? userId = null)
    {
        var template = MilestoneTemplate.Create(
            clientId,
            request.CompanyId,
            request.Name,
            request.Category,
            request.MetricType,
            request.TargetOperator,
            request.MeasurementFrequency,
            request.AccelerationType,
            request.AccelerationAmount,
            request.Description,
            request.MaxAccelerationCap,
            userId);

        await _templateRepository.AddAsync(template);
        return Map(template);
    }

    public async Task<MilestoneTemplateResponse> UpdateAsync(
        Guid id, Guid clientId, UpdateMilestoneTemplateRequest request, Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("MilestoneTemplate", id);

        template.Update(
            request.Name,
            request.Category,
            request.MetricType,
            request.TargetOperator,
            request.MeasurementFrequency,
            request.AccelerationType,
            request.AccelerationAmount,
            request.Description,
            request.MaxAccelerationCap,
            userId);

        await _templateRepository.UpdateAsync(template);
        return Map(template);
    }

    public async Task ActivateAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("MilestoneTemplate", id);
        template.Activate(userId);
        await _templateRepository.UpdateAsync(template);
    }

    public async Task DeactivateAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var template = await _templateRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("MilestoneTemplate", id);
        template.Deactivate(userId);
        await _templateRepository.UpdateAsync(template);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var exists = await _templateRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("MilestoneTemplate", id);
        await _templateRepository.SoftDeleteAsync(id, clientId, userId);
    }

    private static MilestoneTemplateResponse Map(MilestoneTemplate t) =>
        new()
        {
            Id = t.Id,
            ClientId = t.ClientId,
            CompanyId = t.CompanyId,
            Name = t.Name,
            Description = t.Description,
            Category = t.Category,
            MetricType = t.MetricType,
            TargetOperator = t.TargetOperator,
            MeasurementFrequency = t.MeasurementFrequency,
            IsActive = t.IsActive,
            AccelerationType = t.AccelerationType,
            AccelerationAmount = t.AccelerationAmount,
            MaxAccelerationCap = t.MaxAccelerationCap,
            EffectiveCap = t.EffectiveCap,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
}
