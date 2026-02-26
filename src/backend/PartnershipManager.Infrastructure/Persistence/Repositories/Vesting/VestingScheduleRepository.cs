using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class VestingScheduleRepository : IVestingScheduleRepository
{
    private readonly DapperContext _context;

    public VestingScheduleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VestingSchedule>> GetByGrantAsync(Guid clientId, Guid vestingGrantId)
    {
        var sql = @"
            SELECT id, client_id, vesting_grant_id, company_id,
                period_number, schedule_date, shares_to_vest, cumulative_shares,
                percentage_to_vest, status, vested_at,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_schedules
            WHERE client_id = @ClientId AND vesting_grant_id = @VestingGrantId AND is_deleted = 0
            ORDER BY period_number ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingGrantId = vestingGrantId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVestingSchedule);
    }

    public async Task<IEnumerable<VestingSchedule>> GetUpcomingAsync(
        Guid clientId, Guid companyId, DateTime fromDate, DateTime toDate)
    {
        var sql = @"
            SELECT id, client_id, vesting_grant_id, company_id,
                period_number, schedule_date, shares_to_vest, cumulative_shares,
                percentage_to_vest, status, vested_at,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_schedules
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND schedule_date BETWEEN @FromDate AND @ToDate
              AND status = @Status AND is_deleted = 0
            ORDER BY schedule_date ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            Status = (int)VestingScheduleStatus.Pending
        }, _context.Transaction);

        return rows.Select(MapToVestingSchedule);
    }

    public async Task AddRangeAsync(IEnumerable<VestingSchedule> schedules)
    {
        var sql = @"
            INSERT INTO vesting_schedules
                (id, client_id, vesting_grant_id, company_id,
                 period_number, schedule_date, shares_to_vest, cumulative_shares,
                 percentage_to_vest, status, vested_at,
                 created_at, updated_at, is_deleted)
            VALUES
                (@Id, @ClientId, @VestingGrantId, @CompanyId,
                 @PeriodNumber, @ScheduleDate, @SharesToVest, @CumulativeShares,
                 @PercentageToVest, @Status, @VestedAt,
                 @CreatedAt, @UpdatedAt, 0)";

        var parameters = schedules.Select(s => new
        {
            Id = s.Id.ToString(),
            ClientId = s.ClientId.ToString(),
            VestingGrantId = s.VestingGrantId.ToString(),
            CompanyId = s.CompanyId.ToString(),
            s.PeriodNumber,
            s.ScheduleDate,
            s.SharesToVest,
            s.CumulativeShares,
            s.PercentageToVest,
            Status = (int)s.Status,
            s.VestedAt,
            s.CreatedAt,
            s.UpdatedAt
        });

        await _context.Connection.ExecuteAsync(sql, parameters, _context.Transaction);
    }

    public async Task UpdateAsync(VestingSchedule schedule)
    {
        var sql = @"
            UPDATE vesting_schedules SET
                status = @Status,
                vested_at = @VestedAt,
                updated_at = @UpdatedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Status = (int)schedule.Status,
            schedule.VestedAt,
            schedule.UpdatedAt,
            Id = schedule.Id.ToString(),
            ClientId = schedule.ClientId.ToString()
        }, _context.Transaction);
    }

    public async Task DeleteByGrantAsync(Guid vestingGrantId)
    {
        var sql = @"
            UPDATE vesting_schedules
            SET is_deleted = 1, deleted_at = @DeletedAt
            WHERE vesting_grant_id = @VestingGrantId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            VestingGrantId = vestingGrantId.ToString(),
            DeletedAt = DateTime.UtcNow
        }, _context.Transaction);
    }

    private static VestingSchedule MapToVestingSchedule(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);

        var status = row.status is int st ? (VestingScheduleStatus)st : Enum.Parse<VestingScheduleStatus>(row.status.ToString()!);

        return VestingSchedule.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingGrantId: ParseGuid(row.vesting_grant_id),
            companyId: ParseGuid(row.company_id),
            periodNumber: row.period_number is int pn ? pn : Convert.ToInt32(row.period_number),
            scheduleDate: (DateTime)row.schedule_date,
            sharesToVest: row.shares_to_vest is decimal sv ? sv : Convert.ToDecimal(row.shares_to_vest),
            cumulativeShares: row.cumulative_shares is decimal cv ? cv : Convert.ToDecimal(row.cumulative_shares),
            percentageToVest: row.percentage_to_vest is decimal pv ? pv : Convert.ToDecimal(row.percentage_to_vest),
            status: status,
            vestedAt: row.vested_at == null || row.vested_at is DBNull ? null : (DateTime?)row.vested_at,
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool b ? b : Convert.ToBoolean(row.is_deleted),
            deletedAt: row.deleted_at == null || row.deleted_at is DBNull ? null : (DateTime?)row.deleted_at);
    }
}
