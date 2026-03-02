using System.Text.Json;
using PartnershipManager.Application.Features.CustomFormulas.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Services.Valuation;

namespace PartnershipManager.Infrastructure.Services.CustomFormulas;

public interface ICustomFormulaService
{
    Task<CustomFormulaListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        bool? isActive = null, string? sectorTag = null);
    Task<CustomFormulaResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<CustomFormulaResponse> CreateAsync(Guid clientId, CreateCustomFormulaRequest request, Guid userId);
    Task<CustomFormulaResponse> UpdateMetadataAsync(Guid id, Guid clientId, UpdateFormulaMetadataRequest request, Guid userId);
    Task<FormulaVersionResponse> PublishNewVersionAsync(Guid formulaId, Guid clientId,
        PublishNewFormulaVersionRequest request, Guid userId);
    Task<IEnumerable<FormulaVersionResponse>> GetVersionsAsync(Guid formulaId, Guid clientId);
    Task<CustomFormulaResponse> ActivateAsync(Guid id, Guid clientId, Guid userId);
    Task<CustomFormulaResponse> DeactivateAsync(Guid id, Guid clientId, Guid userId);
    Task<TestFormulaResponse> TestFormulaAsync(TestFormulaRequest request);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class CustomFormulaService : ICustomFormulaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomFormulaEngine _formulaEngine;

    public CustomFormulaService(IUnitOfWork unitOfWork, ICustomFormulaEngine formulaEngine)
    {
        _unitOfWork = unitOfWork;
        _formulaEngine = formulaEngine;
    }

    public async Task<CustomFormulaListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        bool? isActive = null, string? sectorTag = null)
    {
        var (items, total) = await _unitOfWork.CustomFormulas.GetPagedAsync(clientId, companyId, page, pageSize, isActive, sectorTag);
        var responses = new List<CustomFormulaResponse>();
        foreach (var f in items)
        {
            FormulaVersionResponse? currentVersion = null;
            if (f.CurrentVersionId.HasValue)
            {
                var v = await _unitOfWork.FormulaVersions.GetByIdAsync(f.CurrentVersionId.Value, clientId);
                if (v != null) currentVersion = MapVersionToResponse(v);
            }
            responses.Add(MapToResponse(f, currentVersion));
        }
        return new CustomFormulaListResponse(responses, total, page, pageSize);
    }

    public async Task<CustomFormulaResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var formula = await _unitOfWork.CustomFormulas.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("CustomFormula", id);

        FormulaVersionResponse? currentVersion = null;
        if (formula.CurrentVersionId.HasValue)
        {
            var v = await _unitOfWork.FormulaVersions.GetByIdAsync(formula.CurrentVersionId.Value, clientId);
            if (v != null) currentVersion = MapVersionToResponse(v);
        }
        return MapToResponse(formula, currentVersion);
    }

    public async Task<CustomFormulaResponse> CreateAsync(Guid clientId, CreateCustomFormulaRequest request, Guid userId)
    {
        // Validate the expression before saving
        _formulaEngine.TryValidate(request.Expression, out _);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var formula = ValuationCustomFormula.Create(
                clientId,
                request.CompanyId,
                request.Name,
                request.Description,
                request.SectorTag,
                userId);

            await _unitOfWork.CustomFormulas.AddAsync(formula);

            // Create first version
            var variablesJson = JsonSerializer.Serialize(request.Variables);
            var version = ValuationFormulaVersion.Create(
                formula.Id,
                clientId,
                1,
                request.Expression,
                variablesJson,
                request.ResultUnit,
                request.ResultLabel,
                userId);

            // Attempt to validate
            if (_formulaEngine.TryValidate(request.Expression, out var errors))
                version.MarkValidated();
            else
                version.MarkInvalid(errors);

            await _unitOfWork.FormulaVersions.AddAsync(version);

            // Link current version to formula
            formula.SetCurrentVersion(version.Id, userId);
            await _unitOfWork.CustomFormulas.UpdateAsync(formula);

            await _unitOfWork.CommitTransactionAsync();
            return MapToResponse(formula, MapVersionToResponse(version));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<CustomFormulaResponse> UpdateMetadataAsync(Guid id, Guid clientId,
        UpdateFormulaMetadataRequest request, Guid userId)
    {
        var formula = await _unitOfWork.CustomFormulas.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("CustomFormula", id);

        formula.UpdateMetadata(request.Name, request.Description, request.SectorTag, userId);
        await _unitOfWork.CustomFormulas.UpdateAsync(formula);

        FormulaVersionResponse? currentVersion = null;
        if (formula.CurrentVersionId.HasValue)
        {
            var v = await _unitOfWork.FormulaVersions.GetByIdAsync(formula.CurrentVersionId.Value, clientId);
            if (v != null) currentVersion = MapVersionToResponse(v);
        }
        return MapToResponse(formula, currentVersion);
    }

    public async Task<FormulaVersionResponse> PublishNewVersionAsync(Guid formulaId, Guid clientId,
        PublishNewFormulaVersionRequest request, Guid userId)
    {
        var formula = await _unitOfWork.CustomFormulas.GetByIdAsync(formulaId, clientId)
            ?? throw new NotFoundException("CustomFormula", formulaId);

        var nextVersionNumber = await _unitOfWork.FormulaVersions.GetNextVersionNumberAsync(formulaId);
        var variablesJson = JsonSerializer.Serialize(request.Variables);

        var version = ValuationFormulaVersion.Create(
            formulaId,
            clientId,
            nextVersionNumber,
            request.Expression,
            variablesJson,
            request.ResultUnit,
            request.ResultLabel,
            userId);

        if (_formulaEngine.TryValidate(request.Expression, out var errors))
            version.MarkValidated();
        else
            version.MarkInvalid(errors);

        await _unitOfWork.FormulaVersions.AddAsync(version);

        // Update formula to point to new version
        formula.SetCurrentVersion(version.Id, userId);
        await _unitOfWork.CustomFormulas.UpdateAsync(formula);

        return MapVersionToResponse(version);
    }

    public async Task<IEnumerable<FormulaVersionResponse>> GetVersionsAsync(Guid formulaId, Guid clientId)
    {
        _ = await _unitOfWork.CustomFormulas.GetByIdAsync(formulaId, clientId)
            ?? throw new NotFoundException("CustomFormula", formulaId);

        var versions = await _unitOfWork.FormulaVersions.GetByFormulaAsync(formulaId, clientId);
        return versions.Select(MapVersionToResponse);
    }

    public async Task<CustomFormulaResponse> ActivateAsync(Guid id, Guid clientId, Guid userId)
    {
        var formula = await _unitOfWork.CustomFormulas.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("CustomFormula", id);

        formula.Activate(userId);
        await _unitOfWork.CustomFormulas.UpdateAsync(formula);

        FormulaVersionResponse? currentVersion = null;
        if (formula.CurrentVersionId.HasValue)
        {
            var v = await _unitOfWork.FormulaVersions.GetByIdAsync(formula.CurrentVersionId.Value, clientId);
            if (v != null) currentVersion = MapVersionToResponse(v);
        }
        return MapToResponse(formula, currentVersion);
    }

    public async Task<CustomFormulaResponse> DeactivateAsync(Guid id, Guid clientId, Guid userId)
    {
        var formula = await _unitOfWork.CustomFormulas.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("CustomFormula", id);

        formula.Deactivate(userId);
        await _unitOfWork.CustomFormulas.UpdateAsync(formula);

        FormulaVersionResponse? currentVersion = null;
        if (formula.CurrentVersionId.HasValue)
        {
            var v = await _unitOfWork.FormulaVersions.GetByIdAsync(formula.CurrentVersionId.Value, clientId);
            if (v != null) currentVersion = MapVersionToResponse(v);
        }
        return MapToResponse(formula, currentVersion);
    }

    public Task<TestFormulaResponse> TestFormulaAsync(TestFormulaRequest request)
    {
        var isValid = _formulaEngine.TryValidate(request.Expression, out var syntaxErrors);

        if (!isValid)
        {
            return Task.FromResult(new TestFormulaResponse
            {
                IsValid = false,
                Result = null,
                Errors = syntaxErrors.ToList()
            });
        }

        try
        {
            var result = _formulaEngine.Evaluate(request.Expression, request.Inputs);
            return Task.FromResult(new TestFormulaResponse
            {
                IsValid = true,
                Result = result,
                Errors = [],
                NormalizedExpression = request.Expression.Trim()
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TestFormulaResponse
            {
                IsValid = false,
                Result = null,
                Errors = [ex.Message]
            });
        }
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        _ = await _unitOfWork.CustomFormulas.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("CustomFormula", id);
        await _unitOfWork.CustomFormulas.SoftDeleteAsync(id, clientId, userId);
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static CustomFormulaResponse MapToResponse(ValuationCustomFormula f, FormulaVersionResponse? currentVersion) =>
        new()
        {
            Id = f.Id,
            ClientId = f.ClientId,
            CompanyId = f.CompanyId,
            Name = f.Name,
            Description = f.Description,
            SectorTag = f.SectorTag,
            CurrentVersionId = f.CurrentVersionId,
            IsActive = f.IsActive,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt,
            CurrentVersion = currentVersion
        };

    private static FormulaVersionResponse MapVersionToResponse(ValuationFormulaVersion v) =>
        new()
        {
            Id = v.Id,
            FormulaId = v.FormulaId,
            VersionNumber = v.VersionNumber,
            Expression = v.Expression,
            Variables = v.GetVariables(),
            ResultUnit = v.ResultUnit,
            ResultLabel = v.ResultLabel,
            TestInputs = v.TestInputs,
            TestResult = v.TestResult,
            ValidationStatus = v.ValidationStatus,
            ValidationErrors = v.ValidationErrors,
            CreatedAt = v.CreatedAt
        };
}
