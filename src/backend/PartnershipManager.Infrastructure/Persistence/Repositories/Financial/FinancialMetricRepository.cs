using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class FinancialMetricRepository : IFinancialMetricRepository
{
    private readonly DapperContext _context;

    public FinancialMetricRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        fm.id, fm.client_id, fm.period_id,
        fm.gross_revenue, fm.net_revenue, fm.mrr, fm.arr,
        fm.cash_balance, fm.burn_rate, fm.runway_months,
        fm.customer_count, fm.churn_rate, fm.cac, fm.ltv, fm.nps,
        fm.ebitda, fm.ebitda_margin, fm.net_income,
        fm.created_at, fm.updated_at, fm.created_by, fm.updated_by";

    public async Task<FinancialMetric?> GetByPeriodAsync(Guid periodId, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM financial_metrics fm
            WHERE fm.period_id = @PeriodId AND fm.client_id = @ClientId";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            PeriodId = periodId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToMetric(row);
    }

    public async Task<IEnumerable<FinancialMetric>> GetByCompanyAsync(
        Guid clientId, Guid companyId, int lastNPeriods = 12)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM financial_metrics fm
            INNER JOIN financial_periods fp ON fp.id = fm.period_id
            WHERE fm.client_id = @ClientId AND fp.company_id = @CompanyId
              AND fp.is_deleted = 0
            ORDER BY fp.year DESC, fp.month DESC
            LIMIT @Limit";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Limit = lastNPeriods
        }, _context.Transaction);

        return rows.Select(MapToMetric);
    }

    public async Task AddAsync(FinancialMetric m)
    {
        const string sql = @"
            INSERT INTO financial_metrics (
                id, client_id, period_id,
                gross_revenue, net_revenue, mrr, arr,
                cash_balance, burn_rate, runway_months,
                customer_count, churn_rate, cac, ltv, nps,
                ebitda, ebitda_margin, net_income,
                created_at, updated_at, created_by, updated_by
            ) VALUES (
                @Id, @ClientId, @PeriodId,
                @GrossRevenue, @NetRevenue, @Mrr, @Arr,
                @CashBalance, @BurnRate, @RunwayMonths,
                @CustomerCount, @ChurnRate, @Cac, @Ltv, @Nps,
                @Ebitda, @EbitdaMargin, @NetIncome,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(m), _context.Transaction);
    }

    public async Task UpdateAsync(FinancialMetric m)
    {
        const string sql = @"
            UPDATE financial_metrics SET
                gross_revenue = @GrossRevenue, net_revenue = @NetRevenue,
                mrr = @Mrr, arr = @Arr,
                cash_balance = @CashBalance, burn_rate = @BurnRate, runway_months = @RunwayMonths,
                customer_count = @CustomerCount, churn_rate = @ChurnRate,
                cac = @Cac, ltv = @Ltv, nps = @Nps,
                ebitda = @Ebitda, ebitda_margin = @EbitdaMargin, net_income = @NetIncome,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId";

        await _context.Connection.ExecuteAsync(sql, ToParams(m), _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────

    private static FinancialMetric MapToMetric(dynamic r)
    {
        var m = (FinancialMetric)Activator.CreateInstance(typeof(FinancialMetric), nonPublic: true)!;
        Set(m, "Id", Guid.Parse((string)r.id));
        Set(m, "ClientId", Guid.Parse((string)r.client_id));
        Set(m, "PeriodId", Guid.Parse((string)r.period_id));
        Set(m, "GrossRevenue", (decimal?)r.gross_revenue);
        Set(m, "NetRevenue", (decimal?)r.net_revenue);
        Set(m, "Mrr", (decimal?)r.mrr);
        Set(m, "Arr", (decimal?)r.arr);
        Set(m, "CashBalance", (decimal?)r.cash_balance);
        Set(m, "BurnRate", (decimal?)r.burn_rate);
        Set(m, "RunwayMonths", (decimal?)r.runway_months);
        Set(m, "CustomerCount", (int?)r.customer_count);
        Set(m, "ChurnRate", (decimal?)r.churn_rate);
        Set(m, "Cac", (decimal?)r.cac);
        Set(m, "Ltv", (decimal?)r.ltv);
        Set(m, "Nps", (short?)r.nps);
        Set(m, "Ebitda", (decimal?)r.ebitda);
        Set(m, "EbitdaMargin", (decimal?)r.ebitda_margin);
        Set(m, "NetIncome", (decimal?)r.net_income);
        Set(m, "CreatedAt", (DateTime)r.created_at);
        Set(m, "UpdatedAt", (DateTime)r.updated_at);
        Set(m, "CreatedBy", r.created_by is null ? (Guid?)null : Guid.Parse((string)r.created_by));
        Set(m, "UpdatedBy", r.updated_by is null ? (Guid?)null : Guid.Parse((string)r.updated_by));
        return m;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(FinancialMetric m) => new
    {
        Id = m.Id.ToString(),
        ClientId = m.ClientId.ToString(),
        PeriodId = m.PeriodId.ToString(),
        m.GrossRevenue,
        m.NetRevenue,
        m.Mrr,
        m.Arr,
        m.CashBalance,
        m.BurnRate,
        m.RunwayMonths,
        m.CustomerCount,
        m.ChurnRate,
        m.Cac,
        m.Ltv,
        m.Nps,
        m.Ebitda,
        m.EbitdaMargin,
        m.NetIncome,
        m.CreatedAt,
        m.UpdatedAt,
        CreatedBy = m.CreatedBy?.ToString(),
        UpdatedBy = m.UpdatedBy?.ToString()
    };
}
