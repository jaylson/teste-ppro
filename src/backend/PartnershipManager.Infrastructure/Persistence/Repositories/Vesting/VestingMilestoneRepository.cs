using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class VestingMilestoneRepository : IVestingMilestoneRepository
{
    private readonly DapperContext _context;

    public VestingMilestoneRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VestingMilestone>> GetByPlanAsync(Guid clientId, Guid vestingPlanId)
    {
        var sql = @"
            SELECT id, client_id, vesting_plan_id, company_id,
                name, description, milestone_type, target_value, target_unit,
                acceleration_percentage, is_required_for_full_vesting,
                status, target_date, achieved_date, achieved_by, achieved_value,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_milestones
            WHERE client_id = @ClientId AND vesting_plan_id = @VestingPlanId AND is_deleted = 0
            ORDER BY target_date ASC, created_at ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingPlanId = vestingPlanId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVestingMilestone);
    }

    public async Task<(IEnumerable<VestingMilestone> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        Guid? vestingPlanId = null, string? status = null)
    {
        var where = "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_deleted = 0";
        if (vestingPlanId.HasValue) where += " AND vesting_plan_id = @VestingPlanId";
        if (!string.IsNullOrWhiteSpace(status)) where += " AND status = @Status";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                id, client_id, vesting_plan_id, company_id,
                name, description, milestone_type, target_value, target_unit,
                acceleration_percentage, is_required_for_full_vesting,
                status, target_date, achieved_date, achieved_by, achieved_value,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_milestones
            {where}
            ORDER BY target_date ASC, created_at ASC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            VestingPlanId = vestingPlanId?.ToString(),
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToVestingMilestone), total);
    }

    public async Task<VestingMilestone?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = @"
            SELECT id, client_id, vesting_plan_id, company_id,
                name, description, milestone_type, target_value, target_unit,
                acceleration_percentage, is_required_for_full_vesting,
                status, target_date, achieved_date, achieved_by, achieved_value,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_milestones
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToVestingMilestone(row);
    }

    public async Task AddAsync(VestingMilestone milestone)
    {
        var sql = @"
            INSERT INTO vesting_milestones
                (id, client_id, vesting_plan_id, company_id,
                 name, description, milestone_type, target_value, target_unit,
                 acceleration_percentage, is_required_for_full_vesting,
                 status, target_date, achieved_date, achieved_by, achieved_value,
                 created_at, updated_at, is_deleted)
            VALUES
                (@Id, @ClientId, @VestingPlanId, @CompanyId,
                 @Name, @Description, @MilestoneType, @TargetValue, @TargetUnit,
                 @AccelerationPercentage, @IsRequiredForFullVesting,
                 @Status, @TargetDate, @AchievedDate, @AchievedBy, @AchievedValue,
                 @CreatedAt, @UpdatedAt, 0)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = milestone.Id.ToString(),
            ClientId = milestone.ClientId.ToString(),
            VestingPlanId = milestone.VestingPlanId.ToString(),
            CompanyId = milestone.CompanyId.ToString(),
            milestone.Name,
            milestone.Description,
            MilestoneType = (int)milestone.MilestoneType,
            milestone.TargetValue,
            milestone.TargetUnit,
            milestone.AccelerationPercentage,
            milestone.IsRequiredForFullVesting,
            Status = (int)milestone.Status,
            milestone.TargetDate,
            milestone.AchievedDate,
            AchievedBy = milestone.AchievedBy?.ToString(),
            milestone.AchievedValue,
            milestone.CreatedAt,
            milestone.UpdatedAt
        }, _context.Transaction);
    }

    public async Task UpdateAsync(VestingMilestone milestone)
    {
        var sql = @"
            UPDATE vesting_milestones SET
                name = @Name,
                description = @Description,
                target_value = @TargetValue,
                target_unit = @TargetUnit,
                acceleration_percentage = @AccelerationPercentage,
                is_required_for_full_vesting = @IsRequiredForFullVesting,
                status = @Status,
                target_date = @TargetDate,
                achieved_date = @AchievedDate,
                achieved_by = @AchievedBy,
                achieved_value = @AchievedValue,
                updated_at = @UpdatedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            milestone.Name,
            milestone.Description,
            milestone.TargetValue,
            milestone.TargetUnit,
            milestone.AccelerationPercentage,
            milestone.IsRequiredForFullVesting,
            Status = (int)milestone.Status,
            milestone.TargetDate,
            milestone.AchievedDate,
            AchievedBy = milestone.AchievedBy?.ToString(),
            milestone.AchievedValue,
            milestone.UpdatedAt,
            Id = milestone.Id.ToString(),
            ClientId = milestone.ClientId.ToString()
        }, _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE vesting_milestones
            SET is_deleted = 1, deleted_at = @DeletedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            DeletedAt = DateTime.UtcNow
        }, _context.Transaction);
    }

    private static VestingMilestone MapToVestingMilestone(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null || value is DBNull ? null
            : (value is Guid g ? g : Guid.Parse(value.ToString()!));

        var milestoneType = row.milestone_type is int mt ? (MilestoneType)mt : Enum.Parse<MilestoneType>(row.milestone_type.ToString()!);
        var status = row.status is int st ? (MilestoneStatus)st : Enum.Parse<MilestoneStatus>(row.status.ToString()!);

        return VestingMilestone.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingPlanId: ParseGuid(row.vesting_plan_id),
            companyId: ParseGuid(row.company_id),
            name: (string)row.name,
            description: row.description == null || row.description is DBNull ? null : (string)row.description,
            milestoneType: milestoneType,
            targetValue: row.target_value == null || row.target_value is DBNull ? null
                : (decimal?)Convert.ToDecimal(row.target_value),
            targetUnit: row.target_unit == null || row.target_unit is DBNull ? null : (string)row.target_unit,
            accelerationPercentage: row.acceleration_percentage is decimal ap ? ap : Convert.ToDecimal(row.acceleration_percentage),
            isRequiredForFullVesting: row.is_required_for_full_vesting is bool rfv ? rfv : Convert.ToBoolean(row.is_required_for_full_vesting),
            status: status,
            targetDate: row.target_date == null || row.target_date is DBNull ? null : (DateTime?)row.target_date,
            achievedDate: row.achieved_date == null || row.achieved_date is DBNull ? null : (DateTime?)row.achieved_date,
            achievedBy: ParseNullableGuid(row.achieved_by),
            achievedValue: row.achieved_value == null || row.achieved_value is DBNull ? null
                : (decimal?)Convert.ToDecimal(row.achieved_value),
            createdBy: ParseNullableGuid(row.created_by),
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool b ? b : Convert.ToBoolean(row.is_deleted),
            deletedAt: row.deleted_at == null || row.deleted_at is DBNull ? null : (DateTime?)row.deleted_at);
    }
}
