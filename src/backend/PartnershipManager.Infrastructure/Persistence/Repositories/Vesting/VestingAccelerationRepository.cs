using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class VestingAccelerationRepository : IVestingAccelerationRepository
{
    private readonly DapperContext _context;

    public VestingAccelerationRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        id, client_id, vesting_grant_id, grant_milestone_id,
        acceleration_type, acceleration_amount,
        original_vesting_end_date, new_vesting_end_date, shares_accelerated,
        applied_at, applied_by, created_at";

    public async Task<IEnumerable<VestingAcceleration>> GetByGrantAsync(
        Guid clientId, Guid vestingGrantId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM vesting_accelerations
            WHERE client_id = @ClientId AND vesting_grant_id = @VestingGrantId
            ORDER BY applied_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingGrantId = vestingGrantId.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task<VestingAcceleration?> GetByMilestoneAsync(
        Guid clientId, Guid grantMilestoneId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM vesting_accelerations
            WHERE client_id = @ClientId AND grant_milestone_id = @MilestoneId
            LIMIT 1";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            MilestoneId = grantMilestoneId.ToString()
        }, _context.Transaction);

        return row is null ? null : Map(row);
    }

    public async Task<decimal> GetTotalAccelerationForGrantAsync(
        Guid clientId, Guid vestingGrantId)
    {
        var sql = @"
            SELECT COALESCE(SUM(acceleration_amount), 0)
            FROM vesting_accelerations
            WHERE client_id = @ClientId AND vesting_grant_id = @VestingGrantId";

        return await _context.Connection.ExecuteScalarAsync<decimal>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingGrantId = vestingGrantId.ToString()
        }, _context.Transaction);
    }

    public async Task AddAsync(VestingAcceleration acceleration)
    {
        var sql = @"
            INSERT INTO vesting_accelerations
                (id, client_id, vesting_grant_id, grant_milestone_id,
                 acceleration_type, acceleration_amount,
                 original_vesting_end_date, new_vesting_end_date, shares_accelerated,
                 applied_at, applied_by, created_at)
            VALUES
                (@Id, @ClientId, @VestingGrantId, @MilestoneId,
                 @AccelerationType, @AccelerationAmount,
                 @OriginalEnd, @NewEnd, @SharesAccelerated,
                 @AppliedAt, @AppliedBy, @CreatedAt)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = acceleration.Id.ToString(),
            ClientId = acceleration.ClientId.ToString(),
            VestingGrantId = acceleration.VestingGrantId.ToString(),
            MilestoneId = acceleration.GrantMilestoneId.ToString(),
            AccelerationType = acceleration.AccelerationType.ToString(),
            acceleration.AccelerationAmount,
            OriginalEnd = acceleration.OriginalVestingEndDate,
            NewEnd = acceleration.NewVestingEndDate,
            acceleration.SharesAccelerated,
            acceleration.AppliedAt,
            AppliedBy = acceleration.AppliedBy.ToString(),
            acceleration.CreatedAt
        }, _context.Transaction);
    }

    private static VestingAcceleration Map(dynamic row)
    {
        static Guid ParseGuid(object v) => v is Guid g ? g : Guid.Parse(v.ToString()!);

        return VestingAcceleration.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingGrantId: ParseGuid(row.vesting_grant_id),
            grantMilestoneId: ParseGuid(row.grant_milestone_id),
            accelerationType: Enum.Parse<VestingAccelerationType>(row.acceleration_type.ToString()!),
            accelerationAmount: Convert.ToDecimal(row.acceleration_amount),
            originalVestingEndDate: (DateTime)row.original_vesting_end_date,
            newVestingEndDate: (DateTime)row.new_vesting_end_date,
            sharesAccelerated: Convert.ToDecimal(row.shares_accelerated),
            appliedAt: (DateTime)row.applied_at,
            appliedBy: ParseGuid(row.applied_by),
            createdAt: (DateTime)row.created_at);
    }
}
