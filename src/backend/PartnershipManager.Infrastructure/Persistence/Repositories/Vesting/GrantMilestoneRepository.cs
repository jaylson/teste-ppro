using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class GrantMilestoneRepository : IGrantMilestoneRepository
{
    private readonly DapperContext _context;

    public GrantMilestoneRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        id, client_id, vesting_grant_id, milestone_template_id, company_id,
        name, description, category, metric_type, target_value, target_operator,
        target_date, measurement_frequency, status, current_value, progress_percentage,
        achieved_at, achieved_value, verified_at, verified_by,
        acceleration_type, acceleration_amount, acceleration_applied, acceleration_applied_at,
        created_by, created_at, updated_at, is_deleted, deleted_at";

    public async Task<IEnumerable<GrantMilestone>> GetByGrantAsync(Guid clientId, Guid vestingGrantId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM grant_milestones
            WHERE client_id = @ClientId AND vesting_grant_id = @VestingGrantId AND is_deleted = 0
            ORDER BY target_date ASC, created_at ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingGrantId = vestingGrantId.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task<(IEnumerable<GrantMilestone> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        Guid? vestingGrantId = null, string? status = null, string? category = null)
    {
        var where = "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_deleted = 0";
        if (vestingGrantId.HasValue) where += " AND vesting_grant_id = @VestingGrantId";
        if (!string.IsNullOrWhiteSpace(status)) where += " AND status = @Status";
        if (!string.IsNullOrWhiteSpace(category)) where += " AND category = @Category";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS {SelectColumns}
            FROM grant_milestones {where}
            ORDER BY target_date ASC, created_at ASC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            VestingGrantId = vestingGrantId?.ToString(),
            Status = status,
            Category = category,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(Map), total);
    }

    public async Task<GrantMilestone?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM grant_milestones
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : Map(row);
    }

    public async Task<IEnumerable<GrantMilestone>> GetPendingAccelerationsAsync(
        Guid clientId, Guid companyId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM grant_milestones
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND status = 'Achieved'
              AND verified_at IS NOT NULL
              AND acceleration_applied = 0
              AND is_deleted = 0
            ORDER BY verified_at ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task AddAsync(GrantMilestone milestone)
    {
        var sql = @"
            INSERT INTO grant_milestones
                (id, client_id, vesting_grant_id, milestone_template_id, company_id,
                 name, description, category, metric_type, target_value, target_operator,
                 target_date, measurement_frequency, status, current_value, progress_percentage,
                 achieved_at, achieved_value, verified_at, verified_by,
                 acceleration_type, acceleration_amount, acceleration_applied, acceleration_applied_at,
                 created_by, updated_by, created_at, updated_at, is_deleted)
            VALUES
                (@Id, @ClientId, @VestingGrantId, @MilestoneTemplateId, @CompanyId,
                 @Name, @Description, @Category, @MetricType, @TargetValue, @TargetOperator,
                 @TargetDate, @MeasurementFrequency, @Status, @CurrentValue, @ProgressPercentage,
                 @AchievedAt, @AchievedValue, @VerifiedAt, @VerifiedBy,
                 @AccelerationType, @AccelerationAmount, @AccelerationApplied, @AccelerationAppliedAt,
                 @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt, 0)";

        await _context.Connection.ExecuteAsync(sql, ToParams(milestone), _context.Transaction);
    }

    public async Task UpdateAsync(GrantMilestone milestone)
    {
        var sql = @"
            UPDATE grant_milestones SET
                name = @Name, description = @Description, category = @Category,
                metric_type = @MetricType, target_value = @TargetValue,
                target_operator = @TargetOperator, target_date = @TargetDate,
                measurement_frequency = @MeasurementFrequency, status = @Status,
                current_value = @CurrentValue, progress_percentage = @ProgressPercentage,
                achieved_at = @AchievedAt, achieved_value = @AchievedValue,
                verified_at = @VerifiedAt, verified_by = @VerifiedBy,
                acceleration_type = @AccelerationType, acceleration_amount = @AccelerationAmount,
                acceleration_applied = @AccelerationApplied,
                acceleration_applied_at = @AccelerationAppliedAt,
                updated_by = @UpdatedBy, updated_at = @UpdatedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(milestone), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE grant_milestones
            SET is_deleted = 1, deleted_at = @DeletedAt, deleted_by = @DeletedBy,
                updated_at = @DeletedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            DeletedBy = deletedBy?.ToString(),
            DeletedAt = DateTime.UtcNow
        }, _context.Transaction);
    }

    private static object ToParams(GrantMilestone m) => new
    {
        Id = m.Id.ToString(),
        ClientId = m.ClientId.ToString(),
        VestingGrantId = m.VestingGrantId.ToString(),
        MilestoneTemplateId = m.MilestoneTemplateId?.ToString(),
        CompanyId = m.CompanyId.ToString(),
        m.Name, m.Description,
        Category = m.Category.ToString(),
        MetricType = m.MetricType.ToString(),
        m.TargetValue,
        TargetOperator = m.TargetOperator.ToString(),
        m.TargetDate,
        MeasurementFrequency = m.MeasurementFrequency.ToString(),
        Status = m.Status.ToString(),
        m.CurrentValue, m.ProgressPercentage,
        m.AchievedAt, m.AchievedValue,
        m.VerifiedAt,
        VerifiedBy = m.VerifiedBy?.ToString(),
        AccelerationType = m.AccelerationType.ToString(),
        m.AccelerationAmount, m.AccelerationApplied, m.AccelerationAppliedAt,
        CreatedBy = m.CreatedBy?.ToString(),
        UpdatedBy = m.UpdatedBy?.ToString(),
        m.CreatedAt, m.UpdatedAt
    };

    private static GrantMilestone Map(dynamic row)
    {
        static Guid ParseGuid(object v) => v is Guid g ? g : Guid.Parse(v.ToString()!);
        static Guid? ParseNullableGuid(object? v) =>
            v == null || v is DBNull ? null : (v is Guid g ? g : Guid.Parse(v.ToString()!));
        static DateTime? ParseNullableDate(object? v) =>
            v == null || v is DBNull ? null : (DateTime?)v;
        static decimal? ParseNullableDecimal(object? v) =>
            v == null || v is DBNull ? null : (decimal?)Convert.ToDecimal(v);

        return GrantMilestone.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingGrantId: ParseGuid(row.vesting_grant_id),
            milestoneTemplateId: ParseNullableGuid(row.milestone_template_id),
            companyId: ParseGuid(row.company_id),
            name: (string)row.name,
            description: row.description == null || row.description is DBNull ? null : (string)row.description,
            category: Enum.Parse<MilestoneCategory>(row.category.ToString()!),
            metricType: Enum.Parse<MetricType>(row.metric_type.ToString()!),
            targetValue: Convert.ToDecimal(row.target_value),
            targetOperator: Enum.Parse<TargetOperator>(row.target_operator.ToString()!),
            targetDate: (DateTime)row.target_date,
            measurementFrequency: Enum.Parse<MeasurementFrequency>(row.measurement_frequency.ToString()!),
            status: Enum.Parse<MilestoneStatus>(row.status.ToString()!),
            currentValue: ParseNullableDecimal(row.current_value),
            progressPercentage: Convert.ToDecimal(row.progress_percentage),
            achievedAt: ParseNullableDate(row.achieved_at),
            achievedValue: ParseNullableDecimal(row.achieved_value),
            verifiedAt: ParseNullableDate(row.verified_at),
            verifiedBy: ParseNullableGuid(row.verified_by),
            accelerationType: Enum.Parse<VestingAccelerationType>(row.acceleration_type.ToString()!),
            accelerationAmount: Convert.ToDecimal(row.acceleration_amount),
            accelerationApplied: row.acceleration_applied is bool b ? b : Convert.ToBoolean(row.acceleration_applied),
            accelerationAppliedAt: ParseNullableDate(row.acceleration_applied_at),
            createdBy: ParseNullableGuid(row.created_by),
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool d ? d : Convert.ToBoolean(row.is_deleted),
            deletedAt: ParseNullableDate(row.deleted_at));
    }
}
