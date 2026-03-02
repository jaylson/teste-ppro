using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IVestingGrantService
{
    Task<VestingGrantListResponse> GetPagedAsync(Guid clientId, Guid? companyId, int page, int pageSize, Guid? vestingPlanId = null, Guid? shareholderId = null, string? status = null);
    Task<VestingGrantResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<IEnumerable<VestingGrantResponse>> GetByShareholderAsync(Guid clientId, Guid shareholderId, Guid? companyId = null);
    Task<VestingGrantResponse> CreateAsync(Guid clientId, CreateVestingGrantRequest request, Guid? userId = null);
    Task<VestingGrantResponse> ApproveAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingGrantResponse> ActivateAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingGrantResponse> CancelAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingGrantResponse> RecalculateVestingAsync(Guid id, Guid clientId, Guid userId);
    Task<VestingTransactionResponse> ExerciseSharesAsync(Guid id, Guid clientId, ExerciseSharesRequest request, Guid userId);
    Task<VestingCalculationResult> CalculateAsOfAsync(Guid id, Guid clientId, DateTime? asOfDate = null);
    Task<VestingProjectionResponse> GetProjectionAsync(Guid id, Guid clientId, int monthsAhead = 24);
    Task<IEnumerable<VestingTransactionResponse>> GetTransactionsAsync(Guid id, Guid clientId);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class VestingGrantService : IVestingGrantService
{
    private readonly IVestingGrantRepository _grantRepository;
    private readonly IVestingPlanRepository _planRepository;
    private readonly IVestingTransactionRepository _transactionRepository;
    private readonly IVestingScheduleRepository _scheduleRepository;
    private readonly IShareholderRepository _shareholderRepository;
    private readonly IShareTransactionRepository _shareTransactionRepository;
    private readonly IShareClassRepository _shareClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VestingGrantService(
        IVestingGrantRepository grantRepository,
        IVestingPlanRepository planRepository,
        IVestingTransactionRepository transactionRepository,
        IVestingScheduleRepository scheduleRepository,
        IShareholderRepository shareholderRepository,
        IShareTransactionRepository shareTransactionRepository,
        IShareClassRepository shareClassRepository,
        IUnitOfWork unitOfWork)
    {
        _grantRepository = grantRepository;
        _planRepository = planRepository;
        _transactionRepository = transactionRepository;
        _scheduleRepository = scheduleRepository;
        _shareholderRepository = shareholderRepository;
        _shareTransactionRepository = shareTransactionRepository;
        _shareClassRepository = shareClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VestingGrantListResponse> GetPagedAsync(
        Guid clientId, Guid? companyId, int page, int pageSize,
        Guid? vestingPlanId = null, Guid? shareholderId = null, string? status = null)
    {
        var (items, total) = await _grantRepository.GetPagedAsync(
            clientId, companyId, page, pageSize, vestingPlanId, shareholderId, status);
        var asOfDate = DateTime.UtcNow;
        var itemsList = items.ToList();

        // Busca nomes do plano e do beneficiário (deduplica por IDs únicos da página)
        var planNameMap = new Dictionary<Guid, string>();
        foreach (var pid in itemsList.Select(g => g.VestingPlanId).Distinct())
        {
            var plan = await _planRepository.GetByIdAsync(pid, clientId);
            if (plan is not null) planNameMap[pid] = plan.Name;
        }

        var shareholderNameMap = new Dictionary<Guid, string>();
        foreach (var sid in itemsList.Select(g => g.ShareholderId).Distinct())
        {
            var sh = await _shareholderRepository.GetByIdAsync(sid, clientId);
            if (sh is not null) shareholderNameMap[sid] = sh.Name;
        }

        return new VestingGrantListResponse(
            itemsList.Select(g => MapToResponse(
                g,
                planNameMap.GetValueOrDefault(g.VestingPlanId),
                shareholderNameMap.GetValueOrDefault(g.ShareholderId),
                asOfDate)),
            total, page, pageSize);
    }

    public async Task<VestingGrantResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);
        var plan = await _planRepository.GetByIdAsync(grant.VestingPlanId, clientId);
        var shareholder = await _shareholderRepository.GetByIdAsync(grant.ShareholderId, clientId);
        return MapToResponse(grant, plan?.Name, shareholder?.Name, DateTime.UtcNow);
    }

    public async Task<IEnumerable<VestingGrantResponse>> GetByShareholderAsync(
        Guid clientId, Guid shareholderId, Guid? companyId = null)
    {
        var grants = (await _grantRepository.GetByShareholderAsync(clientId, shareholderId, companyId)).ToList();
        var asOfDate = DateTime.UtcNow;

        var planNameMap = new Dictionary<Guid, string>();
        foreach (var pid in grants.Select(g => g.VestingPlanId).Distinct())
        {
            var plan = await _planRepository.GetByIdAsync(pid, clientId);
            if (plan is not null) planNameMap[pid] = plan.Name;
        }

        var sh = await _shareholderRepository.GetByIdAsync(shareholderId, clientId);
        var shareholderName = sh?.Name;

        return grants.Select(g => MapToResponse(
            g,
            planNameMap.GetValueOrDefault(g.VestingPlanId),
            shareholderName,
            asOfDate));
    }

    public async Task<VestingGrantResponse> CreateAsync(
        Guid clientId, CreateVestingGrantRequest request, Guid? userId = null)
    {
        var plan = await _planRepository.GetByIdAsync(request.VestingPlanId, clientId)
            ?? throw new NotFoundException("VestingPlan", request.VestingPlanId);

        if (plan.Status != VestingPlanStatus.Active)
            throw new DomainException("Apenas planos de vesting ativos podem receber grants.");

        var shareholder = await _shareholderRepository.GetByIdAsync(request.ShareholderId, clientId)
            ?? throw new NotFoundException("Shareholder", request.ShareholderId);

        var grant = VestingGrant.Create(
            clientId,
            request.VestingPlanId,
            request.ShareholderId,
            plan.CompanyId,
            request.GrantDate,
            request.TotalShares,
            request.SharePrice,
            request.EquityPercentage,
            request.VestingStartDate,
            plan.VestingMonths,
            plan.CliffMonths,
            request.Notes,
            userId);

        await _grantRepository.AddAsync(grant);
        return MapToResponse(grant, plan.Name, shareholder.Name, DateTime.UtcNow);
    }

    public async Task<VestingGrantResponse> ApproveAsync(Guid id, Guid clientId, Guid userId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);
        grant.Approve(userId);
        await _grantRepository.UpdateAsync(grant);
        var plan = await _planRepository.GetByIdAsync(grant.VestingPlanId, clientId);
        var sh = await _shareholderRepository.GetByIdAsync(grant.ShareholderId, clientId);
        return MapToResponse(grant, plan?.Name, sh?.Name, DateTime.UtcNow);
    }

    public async Task<VestingGrantResponse> ActivateAsync(Guid id, Guid clientId, Guid userId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);
        grant.Activate(userId);
        await _grantRepository.UpdateAsync(grant);
        var plan = await _planRepository.GetByIdAsync(grant.VestingPlanId, clientId);
        var sh = await _shareholderRepository.GetByIdAsync(grant.ShareholderId, clientId);
        return MapToResponse(grant, plan?.Name, sh?.Name, DateTime.UtcNow);
    }

    public async Task<VestingGrantResponse> CancelAsync(Guid id, Guid clientId, Guid userId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);
        grant.Cancel(userId);
        await _grantRepository.UpdateAsync(grant);
        var plan = await _planRepository.GetByIdAsync(grant.VestingPlanId, clientId);
        var sh = await _shareholderRepository.GetByIdAsync(grant.ShareholderId, clientId);
        return MapToResponse(grant, plan?.Name, sh?.Name, DateTime.UtcNow);
    }

    public async Task<VestingGrantResponse> RecalculateVestingAsync(Guid id, Guid clientId, Guid userId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);

        grant.RecalculateVestedShares(DateTime.UtcNow, userId);
        await _grantRepository.UpdateAsync(grant);
        var plan = await _planRepository.GetByIdAsync(grant.VestingPlanId, clientId);
        var sh = await _shareholderRepository.GetByIdAsync(grant.ShareholderId, clientId);
        return MapToResponse(grant, plan?.Name, sh?.Name, DateTime.UtcNow);
    }

    public async Task<VestingTransactionResponse> ExerciseSharesAsync(
        Guid id, Guid clientId, ExerciseSharesRequest request, Guid userId)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);

        // Ensure vested shares are up to date
        grant.RecalculateVestedShares(request.ExerciseDate, userId);

        if (!grant.CanExercise(request.SharesToExercise))
            throw new DomainException(
                $"Não é possível exercer {request.SharesToExercise} ações. " +
                $"Disponível para exercício: {grant.AvailableToExercise}");

        grant.ExerciseShares(request.SharesToExercise, userId);

        var txType = request.TransactionType == 0 ? VestingTransactionType.Exercise : request.TransactionType;
        var vestingTx = VestingTransaction.Create(
            clientId,
            grant.Id,
            grant.ShareholderId,
            grant.CompanyId,
            request.ExerciseDate,
            request.SharesToExercise,
            request.SharePriceAtExercise,
            grant.SharePrice,
            userId,
            txType,
            notes: request.Notes);

        // ── Cap Table integration (atomic) ─────────────────────────────────
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _grantRepository.UpdateAsync(grant);
            await _transactionRepository.AddAsync(vestingTx);

            // Criar ShareTransaction no cap table se houver share class na empresa
            var shareClasses = await _shareClassRepository.GetByCompanyAsync(clientId, grant.CompanyId);
            var shareClass = shareClasses.FirstOrDefault();
            if (shareClass is not null)
            {
                var txNumber = await _shareTransactionRepository.GetNextTransactionNumberAsync(grant.CompanyId);
                var shareTx = ShareTransaction.CreateIssuance(
                    clientId,
                    grant.CompanyId,
                    shareClass.Id,
                    grant.ShareholderId,
                    request.SharesToExercise,
                    request.SharePriceAtExercise,
                    request.ExerciseDate,
                    transactionNumber: txNumber,
                    reason: $"Exercício de vesting — Grant {grant.Id}",
                    notes: request.Notes,
                    createdBy: userId);

                await _shareTransactionRepository.AddAsync(shareTx);
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        // ──────────────────────────────────────────────────────────────────

        return new VestingTransactionResponse
        {
            Id = vestingTx.Id,
            VestingGrantId = vestingTx.VestingGrantId,
            ShareholderId = vestingTx.ShareholderId,
            ShareholderName = string.Empty,
            TransactionDate = vestingTx.TransactionDate,
            SharesExercised = vestingTx.SharesExercised,
            SharePriceAtExercise = vestingTx.SharePriceAtExercise,
            StrikePrice = vestingTx.StrikePrice,
            TotalExerciseValue = vestingTx.TotalExerciseValue,
            GainAmount = vestingTx.GainAmount,
            TransactionType = vestingTx.TransactionType,
            Notes = vestingTx.Notes,
            CreatedAt = vestingTx.CreatedAt
        };
    }

    public async Task<VestingCalculationResult> CalculateAsOfAsync(Guid id, Guid clientId, DateTime? asOfDate = null)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);

        var date = asOfDate ?? DateTime.UtcNow;
        var vestedShares = grant.CalculateVestedShares(date);
        var vestedPct = grant.CalculateVestedPercentage(date);

        return new VestingCalculationResult
        {
            GrantId = grant.Id,
            AsOfDate = date,
            TotalShares = grant.TotalShares,
            VestedShares = vestedShares,
            ExercisedShares = grant.ExercisedShares,
            AvailableToExercise = Math.Max(0, vestedShares - grant.ExercisedShares),
            UnvestedShares = grant.TotalShares - vestedShares,
            VestedPercentage = vestedPct,
            IsCliffMet = grant.IsCliffMet(date),
            IsFullyVested = vestedShares >= grant.TotalShares
        };
    }

    public async Task<VestingProjectionResponse> GetProjectionAsync(
        Guid id, Guid clientId, int monthsAhead = 24)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);

        var points = new List<VestingProjectionPoint>();
        var startDate = DateTime.UtcNow.Date;
        var endDate = grant.VestingEndDate > startDate.AddMonths(monthsAhead)
            ? startDate.AddMonths(monthsAhead)
            : grant.VestingEndDate;

        for (var date = startDate; date <= endDate; date = date.AddMonths(1))
        {
            var (vested, pct) = grant.GetFutureProjection(date);
            points.Add(new VestingProjectionPoint
            {
                Date = date,
                VestedShares = vested,
                VestedPercentage = pct
            });
        }

        return new VestingProjectionResponse
        {
            GrantId = grant.Id,
            ProjectionEndDate = endDate,
            Points = points.AsReadOnly()
        };
    }

    public async Task<IEnumerable<VestingTransactionResponse>> GetTransactionsAsync(Guid id, Guid clientId)
    {
        if (!await _grantRepository.ExistsAsync(id, clientId))
            throw new NotFoundException("VestingGrant", id);

        var transactions = await _transactionRepository.GetByGrantAsync(id);
        return transactions.Select(t => new VestingTransactionResponse
        {
            Id = t.Id,
            VestingGrantId = t.VestingGrantId,
            ShareholderId = t.ShareholderId,
            ShareholderName = string.Empty,
            TransactionDate = t.TransactionDate,
            SharesExercised = t.SharesExercised,
            SharePriceAtExercise = t.SharePriceAtExercise,
            StrikePrice = t.StrikePrice,
            TotalExerciseValue = t.TotalExerciseValue,
            GainAmount = t.GainAmount,
            TransactionType = t.TransactionType,
            Notes = t.Notes,
            CreatedAt = t.CreatedAt
        });
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        var grant = await _grantRepository.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("VestingGrant", id);

        if (grant.Status is VestingGrantDetailStatus.Active or VestingGrantDetailStatus.Exercised)
            throw new DomainException("Grants ativos ou exercidos não podem ser excluídos.");

        await _grantRepository.SoftDeleteAsync(id, clientId, userId);
    }

    private static VestingGrantResponse MapToResponse(
        VestingGrant g, string? planName, string? shareholderName, DateTime asOfDate) => new()
    {
        Id = g.Id,
        ClientId = g.ClientId,
        VestingPlanId = g.VestingPlanId,
        VestingPlanName = planName ?? string.Empty,
        ShareholderId = g.ShareholderId,
        ShareholderName = shareholderName ?? string.Empty,
        CompanyId = g.CompanyId,
        GrantDate = g.GrantDate,
        TotalShares = g.TotalShares,
        SharePrice = g.SharePrice,
        EquityPercentage = g.EquityPercentage,
        VestingStartDate = g.VestingStartDate,
        VestingEndDate = g.VestingEndDate,
        CliffDate = g.CliffDate,
        Status = g.Status,
        VestedShares = g.VestedShares,
        ExercisedShares = g.ExercisedShares,
        AvailableToExercise = g.AvailableToExercise,
        VestedPercentage = g.CalculateVestedPercentage(asOfDate),
        ApprovedAt = g.ApprovedAt,
        Notes = g.Notes,
        CreatedAt = g.CreatedAt,
        UpdatedAt = g.UpdatedAt
    };
}
