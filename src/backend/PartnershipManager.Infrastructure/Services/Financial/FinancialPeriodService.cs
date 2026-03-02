using PartnershipManager.Application.Features.Financial.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services.Financial;

public interface IFinancialPeriodService
{
    Task<FinancialPeriodListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        int? year = null, string? status = null);
    Task<FinancialPeriodResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<FinancialPeriodResponse> CreateAsync(Guid clientId, CreateFinancialPeriodRequest request, Guid? userId = null);
    Task<FinancialPeriodResponse> UpdateNotesAsync(Guid id, Guid clientId, UpdateFinancialPeriodRequest request, Guid userId);
    Task<FinancialPeriodResponse> SubmitAsync(Guid id, Guid clientId, Guid userId);
    Task<FinancialPeriodResponse> ApproveAsync(Guid id, Guid clientId, Guid userId);
    Task<FinancialPeriodResponse> LockAsync(Guid id, Guid clientId, Guid userId);
    Task<FinancialPeriodResponse> ReturnToSubmittedAsync(Guid id, Guid clientId, Guid userId);
    Task<FinancialMetricResponse> UpsertRevenueAsync(Guid periodId, Guid clientId, UpsertRevenueRequest request, Guid userId);
    Task<FinancialMetricResponse> UpsertCashBurnAsync(Guid periodId, Guid clientId, UpsertCashBurnRequest request, Guid userId);
    Task<FinancialMetricResponse> UpsertUnitEconomicsAsync(Guid periodId, Guid clientId, UpsertUnitEconomicsRequest request, Guid userId);
    Task<FinancialMetricResponse> UpsertProfitabilityAsync(Guid periodId, Guid clientId, UpsertProfitabilityRequest request, Guid userId);
    Task<FinancialDashboardResponse> GetDashboardAsync(Guid clientId, Guid companyId, int year);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class FinancialPeriodService : IFinancialPeriodService
{
    private readonly IUnitOfWork _unitOfWork;

    public FinancialPeriodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialPeriodListResponse> GetPagedAsync(Guid clientId, Guid companyId, int page, int pageSize,
        int? year = null, string? status = null)
    {
        var (items, total) = await _unitOfWork.FinancialPeriods.GetPagedAsync(clientId, companyId, page, pageSize, year, status);
        var responses = new List<FinancialPeriodResponse>();
        foreach (var p in items)
        {
            var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(p.Id, clientId);
            responses.Add(MapToResponse(p, metrics));
        }
        return new FinancialPeriodListResponse(responses, total, page, pageSize);
    }

    public async Task<FinancialPeriodResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    public async Task<FinancialPeriodResponse> CreateAsync(Guid clientId, CreateFinancialPeriodRequest request, Guid? userId = null)
    {
        // FI-01: unique per company + year + month (DB enforces, but check for a friendlier message)
        var exists = await _unitOfWork.FinancialPeriods.ExistsAsync(clientId, request.CompanyId, request.Year, request.Month);
        if (exists)
            throw new DomainException($"Já existe um período financeiro para {request.Year:D4}/{request.Month:D2}.");

        var period = FinancialPeriod.Create(clientId, request.CompanyId, request.Year, request.Month, request.Notes, userId);
        await _unitOfWork.FinancialPeriods.AddAsync(period);
        return MapToResponse(period, null);
    }

    public async Task<FinancialPeriodResponse> UpdateNotesAsync(Guid id, Guid clientId, UpdateFinancialPeriodRequest request, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);

        period.UpdateNotes(request.Notes, userId);
        await _unitOfWork.FinancialPeriods.UpdateAsync(period);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    public async Task<FinancialPeriodResponse> SubmitAsync(Guid id, Guid clientId, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);

        period.Submit(userId);
        await _unitOfWork.FinancialPeriods.UpdateAsync(period);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    public async Task<FinancialPeriodResponse> ApproveAsync(Guid id, Guid clientId, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);

        // FI-02: previous consecutive period must be approved
        var prev = await _unitOfWork.FinancialPeriods.GetPreviousPeriodAsync(clientId, period.CompanyId, period.Year, period.Month);
        if (prev != null && !prev.IsApproved && !prev.IsLocked)
        {
            throw new DomainException(
                $"Não é possível aprovar {period.PeriodLabel} antes de aprovar o período anterior {prev.PeriodLabel} (FI-02).");
        }

        period.Approve(userId);
        await _unitOfWork.FinancialPeriods.UpdateAsync(period);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    public async Task<FinancialPeriodResponse> LockAsync(Guid id, Guid clientId, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);

        period.Lock(userId);
        await _unitOfWork.FinancialPeriods.UpdateAsync(period);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    public async Task<FinancialPeriodResponse> ReturnToSubmittedAsync(Guid id, Guid clientId, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);

        period.ReturnToSubmitted(userId);
        await _unitOfWork.FinancialPeriods.UpdateAsync(period);
        var metrics = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(id, clientId);
        return MapToResponse(period, metrics);
    }

    // ─── Upsert metrics ──────────────────────────────────────────────────────

    public async Task<FinancialMetricResponse> UpsertRevenueAsync(Guid periodId, Guid clientId, UpsertRevenueRequest request, Guid userId)
    {
        var (period, metric) = await GetPeriodWithEditableMetric(periodId, clientId, userId);
        metric.UpdateRevenue(request.GrossRevenue, request.NetRevenue, request.Mrr, userId);
        await SaveMetric(period.Id, metric);
        return MapMetricToResponse(metric);
    }

    public async Task<FinancialMetricResponse> UpsertCashBurnAsync(Guid periodId, Guid clientId, UpsertCashBurnRequest request, Guid userId)
    {
        var (period, metric) = await GetPeriodWithEditableMetric(periodId, clientId, userId);
        metric.UpdateCashBurn(request.CashBalance, request.BurnRate, userId);
        await SaveMetric(period.Id, metric);
        return MapMetricToResponse(metric);
    }

    public async Task<FinancialMetricResponse> UpsertUnitEconomicsAsync(Guid periodId, Guid clientId, UpsertUnitEconomicsRequest request, Guid userId)
    {
        var (period, metric) = await GetPeriodWithEditableMetric(periodId, clientId, userId);
        metric.UpdateUnitEconomics(request.CustomerCount, request.ChurnRate, request.Cac, request.Ltv, request.Nps, userId);
        await SaveMetric(period.Id, metric);
        return MapMetricToResponse(metric);
    }

    public async Task<FinancialMetricResponse> UpsertProfitabilityAsync(Guid periodId, Guid clientId, UpsertProfitabilityRequest request, Guid userId)
    {
        var (period, metric) = await GetPeriodWithEditableMetric(periodId, clientId, userId);
        metric.UpdateProfitability(request.Ebitda, request.NetIncome, userId);
        await SaveMetric(period.Id, metric);
        return MapMetricToResponse(metric);
    }

    public async Task<FinancialDashboardResponse> GetDashboardAsync(Guid clientId, Guid companyId, int year)
    {
        var periods = await _unitOfWork.FinancialPeriods.GetByYearAsync(clientId, companyId, (short)year);
        var periodList = periods.ToList();

        var responses = new List<FinancialPeriodResponse>();
        foreach (var p in periodList)
        {
            var m = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(p.Id, clientId);
            responses.Add(MapToResponse(p, m));
        }

        var trend = BuildTrend(responses);
        return new FinancialDashboardResponse
        {
            CompanyId = companyId,
            Year = year,
            Periods = responses,
            Trend = trend
        };
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        _ = await _unitOfWork.FinancialPeriods.GetByIdAsync(id, clientId)
            ?? throw new NotFoundException("FinancialPeriod", id);
        await _unitOfWork.FinancialPeriods.SoftDeleteAsync(id, clientId, userId);
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task<(FinancialPeriod, FinancialMetric)> GetPeriodWithEditableMetric(Guid periodId, Guid clientId, Guid userId)
    {
        var period = await _unitOfWork.FinancialPeriods.GetByIdAsync(periodId, clientId)
            ?? throw new NotFoundException("FinancialPeriod", periodId);

        if (!period.CanBeEdited)
            throw new DomainException($"Período {period.PeriodLabel} está bloqueado e não pode ser editado (FI-03).");

        var metric = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(periodId, clientId);
        if (metric == null)
        {
            metric = FinancialMetric.Create(clientId, periodId, userId);
        }
        return (period, metric);
    }

    private async Task SaveMetric(Guid periodId, FinancialMetric metric)
    {
        var existing = await _unitOfWork.FinancialMetrics.GetByPeriodAsync(periodId, metric.ClientId);
        if (existing == null)
            await _unitOfWork.FinancialMetrics.AddAsync(metric);
        else
            await _unitOfWork.FinancialMetrics.UpdateAsync(metric);
    }

    private static FinancialTrendResponse? BuildTrend(List<FinancialPeriodResponse> periods)
    {
        var withMetrics = periods
            .Where(p => p.Metrics != null)
            .OrderBy(p => p.Year).ThenBy(p => p.Month)
            .ToList();

        if (withMetrics.Count == 0) return null;

        var latest = withMetrics.Last();
        var previous = withMetrics.Count >= 2 ? withMetrics[^2] : null;

        decimal? mrrGrowth = null;
        if (latest.Metrics?.Mrr.HasValue == true && previous?.Metrics?.Mrr.HasValue == true && previous.Metrics.Mrr > 0)
            mrrGrowth = Math.Round(((latest.Metrics.Mrr!.Value - previous.Metrics.Mrr!.Value) / previous.Metrics.Mrr!.Value) * 100, 2);

        var last3 = withMetrics.TakeLast(3).ToList();
        var avgBurn = last3.Where(p => p.Metrics?.BurnRate.HasValue == true).Select(p => p.Metrics!.BurnRate!.Value).DefaultIfEmpty(0).Average();
        var avgChurn = last3.Where(p => p.Metrics?.ChurnRate.HasValue == true).Select(p => p.Metrics!.ChurnRate!.Value).DefaultIfEmpty(0).Average();

        return new FinancialTrendResponse
        {
            MrrGrowthPercent = mrrGrowth,
            ArrCurrentMonth = latest.Metrics?.Arr,
            AvgBurnRate3Months = avgBurn > 0 ? Math.Round((decimal)avgBurn, 2) : null,
            RunwayMonths = latest.Metrics?.RunwayMonths,
            RunwayStatus = latest.Metrics?.RunwayStatus,
            AvgChurnRate3Months = avgChurn > 0 ? Math.Round((decimal)avgChurn, 2) : null
        };
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static FinancialPeriodResponse MapToResponse(FinancialPeriod p, FinancialMetric? metrics) =>
        new()
        {
            Id = p.Id,
            ClientId = p.ClientId,
            CompanyId = p.CompanyId,
            Year = p.Year,
            Month = p.Month,
            PeriodLabel = $"{System.Globalization.CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetAbbreviatedMonthName(p.Month)}/{p.Year:D4}",
            Status = p.Status,
            Notes = p.Notes,
            SubmittedAt = p.SubmittedAt,
            ApprovedAt = p.ApprovedAt,
            LockedAt = p.LockedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Metrics = metrics != null ? MapMetricToResponse(metrics) : null
        };

    private static FinancialMetricResponse MapMetricToResponse(FinancialMetric m) =>
        new()
        {
            Id = m.Id,
            PeriodId = m.PeriodId,
            GrossRevenue = m.GrossRevenue,
            NetRevenue = m.NetRevenue,
            Mrr = m.Mrr,
            Arr = m.Arr,
            CashBalance = m.CashBalance,
            BurnRate = m.BurnRate,
            RunwayMonths = m.RunwayMonths,
            RunwayStatus = m.RunwayStatus,
            CustomerCount = m.CustomerCount,
            ChurnRate = m.ChurnRate,
            Cac = m.Cac,
            Ltv = m.Ltv,
            LtvToCacRatio = m.LtvToCacRatio,
            Nps = m.Nps,
            Ebitda = m.Ebitda,
            EbitdaMargin = m.EbitdaMargin,
            NetIncome = m.NetIncome,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
}
