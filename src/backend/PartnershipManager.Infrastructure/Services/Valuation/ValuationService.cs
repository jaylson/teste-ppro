using System.Text.Json;
using PartnershipManager.Application.Features.Valuation.DTOs;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;
using DomainValuation = PartnershipManager.Domain.Entities.Valuation;
using DomainValuationMethod = PartnershipManager.Domain.Entities.ValuationMethod;
using DomainFormulaVariableDefinition = PartnershipManager.Domain.Entities.FormulaVariableDefinition;

namespace PartnershipManager.Infrastructure.Services.Valuation;

public interface IValuationService
{
    Task<ValuationListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        string? status = null, string? eventType = null);
    Task<ValuationResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<ValuationResponse> CreateAsync(Guid clientId, CreateValuationRequest request, Guid? userId = null);
    Task<ValuationResponse> UpdateAsync(Guid id, Guid clientId, UpdateValuationRequest request, Guid userId);
    Task<ValuationResponse> SubmitAsync(Guid id, Guid clientId, Guid userId);
    Task<ValuationResponse> ApproveAsync(Guid id, Guid clientId, Guid userId);
    Task<ValuationResponse> RejectAsync(Guid id, Guid clientId, string reason, Guid userId);
    Task<ValuationResponse> ReturnToDraftAsync(Guid id, Guid clientId, Guid userId);
    Task<ValuationMethodResponse> AddMethodAsync(Guid valuationId, Guid clientId, AddValuationMethodRequest request, Guid userId);
    Task<CalculateMethodResponse> CalculateMethodAsync(Guid valuationId, Guid methodId, Guid clientId,
        CalculateMethodRequest request, Guid userId);
    Task<ValuationResponse> SelectMethodAsync(Guid valuationId, Guid clientId, Guid methodId, Guid userId);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class ValuationService : IValuationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValuationCalculationEngine _calculationEngine;
    private readonly IFormulaVersionRepository _formulaVersionRepo;

    public ValuationService(
        IUnitOfWork unitOfWork,
        IValuationCalculationEngine calculationEngine)
    {
        _unitOfWork = unitOfWork;
        _calculationEngine = calculationEngine;
        _formulaVersionRepo = unitOfWork.FormulaVersions;
    }

    public async Task<ValuationListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        string? status = null, string? eventType = null)
    {
        var (items, total) = await _unitOfWork.Valuations.GetPagedAsync(clientId, companyId, page, pageSize, status, eventType);
        var responses = new List<ValuationResponse>();
        foreach (var v in items)
        {
            var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(v.Id, clientId);
            responses.Add(MapToResponse(v, methods));
        }
        return new ValuationListResponse(responses, total, page, pageSize);
    }

    public async Task<ValuationResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationResponse> CreateAsync(Guid clientId, CreateValuationRequest request, Guid? userId = null)
    {
        var valuation = DomainValuation.Create(
            clientId,
            request.CompanyId,
            request.ValuationDate,
            request.EventType,
            request.TotalShares,
            request.EventName,
            request.Notes,
            userId);

        await _unitOfWork.Valuations.AddAsync(valuation);
        return MapToResponse(valuation, Enumerable.Empty<DomainValuationMethod>());
    }

    public async Task<ValuationResponse> UpdateAsync(Guid id, Guid clientId, UpdateValuationRequest request, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        valuation.UpdateDetails(
            request.ValuationDate,
            request.EventType,
            request.TotalShares,
            request.EventName,
            request.Notes,
            userId);

        await _unitOfWork.Valuations.UpdateAsync(valuation);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationResponse> SubmitAsync(Guid id, Guid clientId, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        // Ensure there is a selected method with a calculated value
        var selectedMethod = await _unitOfWork.ValuationMethods.GetSelectedAsync(id, clientId);
        if (selectedMethod is null || !selectedMethod.CalculatedValue.HasValue)
            throw new DomainException("Selecione uma metodologia calculada antes de submeter o valuation.");

        // Set the valuation amount from the selected method
        valuation.SetValuationAmount(selectedMethod.CalculatedValue.Value, userId);
        valuation.Submit(userId);

        await _unitOfWork.Valuations.UpdateAsync(valuation);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationResponse> ApproveAsync(Guid id, Guid clientId, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        valuation.Approve(userId);
        await _unitOfWork.Valuations.UpdateAsync(valuation);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationResponse> RejectAsync(Guid id, Guid clientId, string reason, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        valuation.Reject(userId, reason);
        await _unitOfWork.Valuations.UpdateAsync(valuation);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationResponse> ReturnToDraftAsync(Guid id, Guid clientId, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        valuation.ReturnToDraft(userId);
        await _unitOfWork.Valuations.UpdateAsync(valuation);
        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(id, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task<ValuationMethodResponse> AddMethodAsync(Guid valuationId, Guid clientId,
        AddValuationMethodRequest request, Guid userId)
    {
        _ = await _unitOfWork.Valuations.GetByIdAsync(valuationId, clientId)
            ?? throw new NotFoundException("Valuation", valuationId);

        var method = DomainValuationMethod.Create(
            clientId,
            valuationId,
            request.MethodType,
            request.InputsJson,
            request.DataSource,
            request.Notes,
            request.FormulaVersionId,
            userId);

        await _unitOfWork.ValuationMethods.AddAsync(method);
        return MapMethodToResponse(method);
    }

    public async Task<CalculateMethodResponse> CalculateMethodAsync(Guid valuationId, Guid methodId, Guid clientId,
        CalculateMethodRequest request, Guid userId)
    {
        _ = await _unitOfWork.Valuations.GetByIdAsync(valuationId, clientId)
            ?? throw new NotFoundException("Valuation", valuationId);

        var method = await _unitOfWork.ValuationMethods.GetByIdAsync(methodId, clientId)
            ?? throw new NotFoundException("ValuationMethod", methodId);

        string? formulaExpression = null;
        IEnumerable<DomainFormulaVariableDefinition>? formulaVariables = null;

        if (request.FormulaVersionId.HasValue)
        {
            var version = await _formulaVersionRepo.GetByIdAsync(request.FormulaVersionId.Value, clientId)
                ?? throw new NotFoundException("FormulaVersion", request.FormulaVersionId.Value);
            formulaExpression = version.Expression;
            formulaVariables = version.GetVariables();
        }

        var result = await _calculationEngine.CalculateAsync(
            request.MethodType,
            request.Inputs,
            formulaExpression,
            formulaVariables);

        // Persist calculated value back to the method
        method.SetCalculatedValue(result.CalculatedValue, userId);
        // Store inputs snapshot
        var inputsJson = JsonSerializer.Serialize(request.Inputs);
        method.UpdateInputs(inputsJson, method.DataSource, method.Notes, userId);
        await _unitOfWork.ValuationMethods.UpdateAsync(method);

        return result;
    }

    public async Task<ValuationResponse> SelectMethodAsync(Guid valuationId, Guid clientId, Guid methodId, Guid userId)
    {
        var valuation = await _unitOfWork.Valuations.GetByIdAsync(valuationId, clientId)
            ?? throw new NotFoundException("Valuation", valuationId);

        var method = await _unitOfWork.ValuationMethods.GetByIdAsync(methodId, clientId)
            ?? throw new NotFoundException("ValuationMethod", methodId);

        if (!method.CalculatedValue.HasValue)
            throw new DomainException("A metodologia deve ter um valor calculado antes de ser selecionada.");

        // Update the selected method in the repository (clears others, sets this one)
        await _unitOfWork.ValuationMethods.SetSelectedAsync(valuationId, methodId, clientId, userId);

        // Update the valuation amount to the selected method's value
        valuation.SetValuationAmount(method.CalculatedValue.Value, userId);
        await _unitOfWork.Valuations.UpdateAsync(valuation);

        var methods = await _unitOfWork.ValuationMethods.GetByValuationAsync(valuationId, clientId);
        return MapToResponse(valuation, methods);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        _ = await _unitOfWork.Valuations.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("Valuation", id);

        await _unitOfWork.Valuations.SoftDeleteAsync(id, clientId, userId);
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static ValuationResponse MapToResponse(DomainValuation v, IEnumerable<DomainValuationMethod> methods) =>
        new()
        {
            Id = v.Id,
            ClientId = v.ClientId,
            CompanyId = v.CompanyId,
            ValuationDate = v.ValuationDate,
            EventType = v.EventType,
            EventName = v.EventName,
            ValuationAmount = v.ValuationAmount,
            TotalShares = v.TotalShares,
            PricePerShare = v.PricePerShare,
            Status = v.Status,
            Notes = v.Notes,
            SubmittedAt = v.SubmittedAt,
            ApprovedAt = v.ApprovedAt,
            RejectedAt = v.RejectedAt,
            RejectionReason = v.RejectionReason,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            Methods = methods.Select(MapMethodToResponse).ToList()
        };

    private static ValuationMethodResponse MapMethodToResponse(DomainValuationMethod m) =>
        new()
        {
            Id = m.Id,
            ValuationId = m.ValuationId,
            MethodType = m.MethodType,
            IsSelected = m.IsSelected,
            CalculatedValue = m.CalculatedValue,
            Inputs = m.Inputs,
            DataSource = m.DataSource,
            Notes = m.Notes,
            FormulaVersionId = m.FormulaVersionId,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
}
