using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class MilestoneTemplateRepository : IMilestoneTemplateRepository
{
    private readonly DapperContext _context;

    public MilestoneTemplateRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        id, client_id, company_id, name, description,
        category, metric_type, target_operator, target_value, target_unit,
        measurement_frequency, is_active,
        acceleration_type, acceleration_amount, max_acceleration_cap,
        created_by, created_at, updated_at, is_deleted, deleted_at";

    public async Task<IEnumerable<MilestoneTemplate>> GetByCompanyAsync(
        Guid clientId, Guid companyId, bool activeOnly = true)
    {
        var where = activeOnly
            ? "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_active = 1 AND is_deleted = 0"
            : "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_deleted = 0";

        var sql = $"SELECT {SelectColumns} FROM milestone_templates {where} ORDER BY category, name";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task<IEnumerable<MilestoneTemplate>> GetByCategoryAsync(
        Guid clientId, Guid companyId, MilestoneCategory category)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM milestone_templates
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND category = @Category AND is_active = 1 AND is_deleted = 0
            ORDER BY name";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Category = category.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task<(IEnumerable<MilestoneTemplate> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? category = null, bool? isActive = null)
    {
        var where = "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_deleted = 0";
        if (!string.IsNullOrWhiteSpace(category)) where += " AND category = @Category";
        if (isActive.HasValue) where += " AND is_active = @IsActive";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS {SelectColumns}
            FROM milestone_templates {where}
            ORDER BY category, name
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Category = category,
            IsActive = isActive,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(Map), total);
    }

    public async Task<MilestoneTemplate?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM milestone_templates
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : Map(row);
    }

    public async Task AddAsync(MilestoneTemplate template)
    {
        var sql = @"
            INSERT INTO milestone_templates
                (id, client_id, company_id, name, description,
                 category, metric_type, target_operator, target_value, target_unit,
                 measurement_frequency, is_active,
                 acceleration_type, acceleration_amount, max_acceleration_cap,
                 created_by, updated_by, created_at, updated_at, is_deleted)
            VALUES
                (@Id, @ClientId, @CompanyId, @Name, @Description,
                 @Category, @MetricType, @TargetOperator, @TargetValue, @TargetUnit,
                 @MeasurementFrequency, @IsActive,
                 @AccelerationType, @AccelerationAmount, @MaxAccelerationCap,
                 @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt, 0)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = template.Id.ToString(),
            ClientId = template.ClientId.ToString(),
            CompanyId = template.CompanyId.ToString(),
            template.Name,
            template.Description,
            Category = template.Category.ToString(),
            MetricType = template.MetricType.ToString(),
            TargetOperator = template.TargetOperator.ToString(),
            MeasurementFrequency = template.MeasurementFrequency.ToString(),
            template.IsActive,
            AccelerationType = template.AccelerationType.ToString(),
            template.AccelerationAmount,
            template.MaxAccelerationCap,
            CreatedBy = template.CreatedBy?.ToString(),
            UpdatedBy = template.UpdatedBy?.ToString(),
            template.CreatedAt,
            template.UpdatedAt
        }, _context.Transaction);
    }

    public async Task UpdateAsync(MilestoneTemplate template)
    {
        var sql = @"
            UPDATE milestone_templates SET
                name = @Name, description = @Description,
                category = @Category, metric_type = @MetricType,
                target_operator = @TargetOperator, target_value = @TargetValue, target_unit = @TargetUnit,
                measurement_frequency = @MeasurementFrequency,
                is_active = @IsActive,
                acceleration_type = @AccelerationType, acceleration_amount = @AccelerationAmount,
                max_acceleration_cap = @MaxAccelerationCap,
                updated_by = @UpdatedBy, updated_at = @UpdatedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            template.Name,
            template.Description,
            Category = template.Category.ToString(),
            MetricType = template.MetricType.ToString(),
            TargetOperator = template.TargetOperator.ToString(),
            template.TargetValue,
            template.TargetUnit,
            MeasurementFrequency = template.MeasurementFrequency.ToString(),
            template.IsActive,
            AccelerationType = template.AccelerationType.ToString(),
            template.AccelerationAmount,
            template.MaxAccelerationCap,
            UpdatedBy = template.UpdatedBy?.ToString(),
            template.UpdatedAt,
            Id = template.Id.ToString(),
            ClientId = template.ClientId.ToString()
        }, _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE milestone_templates
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

    private static MilestoneTemplate Map(dynamic row)
    {
        static Guid ParseGuid(object v) => v is Guid g ? g : Guid.Parse(v.ToString()!);
        static Guid? ParseNullableGuid(object? v) =>
            v == null || v is DBNull ? null : (v is Guid g ? g : Guid.Parse(v.ToString()!));

        return MilestoneTemplate.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            companyId: ParseGuid(row.company_id),
            name: (string)row.name,
            description: row.description == null || row.description is DBNull ? null : (string)row.description,
            category: Enum.Parse<MilestoneCategory>(row.category.ToString()!),
            metricType: Enum.Parse<MetricType>(row.metric_type.ToString()!),
            targetOperator: Enum.Parse<TargetOperator>(row.target_operator.ToString()!),
            targetValue: row.target_value == null || row.target_value is DBNull
                ? null : (decimal?)Convert.ToDecimal(row.target_value),
            targetUnit: row.target_unit == null || row.target_unit is DBNull ? null : (string)row.target_unit,
            measurementFrequency: Enum.Parse<MeasurementFrequency>(row.measurement_frequency.ToString()!),
            isActive: row.is_active is bool b ? b : Convert.ToBoolean(row.is_active),
            accelerationType: Enum.Parse<VestingAccelerationType>(row.acceleration_type.ToString()!),
            accelerationAmount: Convert.ToDecimal(row.acceleration_amount),
            maxAccelerationCap: row.max_acceleration_cap == null || row.max_acceleration_cap is DBNull
                ? null : (decimal?)Convert.ToDecimal(row.max_acceleration_cap),
            createdBy: ParseNullableGuid(row.created_by),
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool d ? d : Convert.ToBoolean(row.is_deleted),
            deletedAt: row.deleted_at == null || row.deleted_at is DBNull ? null : (DateTime?)row.deleted_at);
    }
}
