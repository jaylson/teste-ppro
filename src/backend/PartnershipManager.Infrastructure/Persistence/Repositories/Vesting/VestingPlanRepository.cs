using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class VestingPlanRepository : IVestingPlanRepository
{
    private readonly DapperContext _context;

    public VestingPlanRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<VestingPlan> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? search = null, string? status = null)
    {
        var where = "WHERE vp.client_id = @ClientId AND vp.company_id = @CompanyId AND vp.is_deleted = 0";
        if (!string.IsNullOrWhiteSpace(search))
            where += " AND (vp.name LIKE @Search OR vp.description LIKE @Search)";
        if (!string.IsNullOrWhiteSpace(status))
            where += " AND vp.status = @Status";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                vp.id, vp.client_id, vp.company_id, vp.name, vp.description,
                vp.vesting_type, vp.cliff_months, vp.vesting_months, vp.total_equity_percentage,
                vp.status, vp.activated_at, vp.activated_by,
                vp.created_by, vp.created_at, vp.updated_at, vp.updated_by,
                vp.is_deleted, vp.deleted_at
            FROM vesting_plans vp
            {where}
            ORDER BY vp.created_at DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Search = $"%{search}%",
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        var items = rows.Select(MapToVestingPlan);
        return (items, total);
    }

    public async Task<VestingPlan?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = @"
            SELECT id, client_id, company_id, name, description,
                vesting_type, cliff_months, vesting_months, total_equity_percentage,
                status, activated_at, activated_by,
                created_by, created_at, updated_at, updated_by,
                is_deleted, deleted_at
            FROM vesting_plans
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToVestingPlan(row);
    }

    public async Task<IEnumerable<VestingPlan>> GetByCompanyAsync(Guid clientId, Guid companyId, string? status = null)
    {
        var where = "WHERE client_id = @ClientId AND company_id = @CompanyId AND is_deleted = 0";
        if (!string.IsNullOrWhiteSpace(status))
            where += " AND status = @Status";

        var sql = $@"
            SELECT id, client_id, company_id, name, description,
                vesting_type, cliff_months, vesting_months, total_equity_percentage,
                status, activated_at, activated_by,
                created_by, created_at, updated_at, updated_by,
                is_deleted, deleted_at
            FROM vesting_plans
            {where}
            ORDER BY created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Status = status
        }, _context.Transaction);

        return rows.Select(MapToVestingPlan);
    }

    public async Task AddAsync(VestingPlan plan)
    {
        var sql = @"
            INSERT INTO vesting_plans
                (id, client_id, company_id, name, description, vesting_type,
                 cliff_months, vesting_months, total_equity_percentage,
                 status, activated_at, activated_by,
                 created_by, created_at, updated_at, updated_by, is_deleted)
            VALUES
                (@Id, @ClientId, @CompanyId, @Name, @Description, @VestingType,
                 @CliffMonths, @VestingMonths, @TotalEquityPercentage,
                 @Status, @ActivatedAt, @ActivatedBy,
                 @CreatedBy, @CreatedAt, @UpdatedAt, @UpdatedBy, 0)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = plan.Id.ToString(),
            ClientId = plan.ClientId.ToString(),
            CompanyId = plan.CompanyId.ToString(),
            plan.Name,
            plan.Description,
            VestingType = plan.VestingType.ToString(),
            plan.CliffMonths,
            plan.VestingMonths,
            plan.TotalEquityPercentage,
            Status = plan.Status.ToString(),
            plan.ActivatedAt,
            ActivatedBy = plan.ActivatedBy?.ToString(),
            CreatedBy = plan.CreatedBy?.ToString(),
            plan.CreatedAt,
            plan.UpdatedAt,
            UpdatedBy = plan.UpdatedBy?.ToString()
        }, _context.Transaction);
    }

    public async Task UpdateAsync(VestingPlan plan)
    {
        var sql = @"
            UPDATE vesting_plans SET
                name = @Name,
                description = @Description,
                vesting_type = @VestingType,
                cliff_months = @CliffMonths,
                vesting_months = @VestingMonths,
                total_equity_percentage = @TotalEquityPercentage,
                status = @Status,
                activated_at = @ActivatedAt,
                activated_by = @ActivatedBy,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            plan.Name,
            plan.Description,
            VestingType = plan.VestingType.ToString(),
            plan.CliffMonths,
            plan.VestingMonths,
            plan.TotalEquityPercentage,
            Status = plan.Status.ToString(),
            plan.ActivatedAt,
            ActivatedBy = plan.ActivatedBy?.ToString(),
            plan.UpdatedAt,
            UpdatedBy = plan.UpdatedBy?.ToString(),
            Id = plan.Id.ToString(),
            ClientId = plan.ClientId.ToString()
        }, _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE vesting_plans
            SET is_deleted = 1, deleted_at = @DeletedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            DeletedAt = DateTime.UtcNow
        }, _context.Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid clientId)
    {
        var sql = "SELECT COUNT(1) FROM vesting_plans WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";
        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);
        return count > 0;
    }

    public async Task<bool> NameExistsAsync(Guid clientId, Guid companyId, string name, Guid? excludeId = null)
    {
        var sql = @"SELECT COUNT(1) FROM vesting_plans
                    WHERE client_id = @ClientId AND company_id = @CompanyId AND name = @Name
                    AND is_deleted = 0";
        if (excludeId.HasValue)
            sql += " AND id != @ExcludeId";

        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Name = name,
            ExcludeId = excludeId?.ToString()
        }, _context.Transaction);
        return count > 0;
    }

    private static VestingPlan MapToVestingPlan(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null || value is DBNull ? null
            : (value is Guid g ? g : Guid.Parse(value.ToString()!));

        var vestingType = row.vesting_type is int vt ? (VestingType)vt : Enum.Parse<VestingType>(row.vesting_type.ToString()!);
        var status = row.status is int st ? (VestingPlanStatus)st : Enum.Parse<VestingPlanStatus>(row.status.ToString()!);

        return VestingPlan.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            companyId: ParseGuid(row.company_id),
            name: (string)row.name,
            description: row.description == null || row.description is DBNull ? null : (string)row.description,
            vestingType: vestingType,
            cliffMonths: row.cliff_months is int cm ? cm : Convert.ToInt32(row.cliff_months),
            vestingMonths: row.vesting_months is int vm ? vm : Convert.ToInt32(row.vesting_months),
            totalEquityPercentage: row.total_equity_percentage is decimal tep ? tep : Convert.ToDecimal(row.total_equity_percentage),
            status: status,
            activatedAt: row.activated_at == null || row.activated_at is DBNull ? null : (DateTime?)row.activated_at,
            activatedBy: ParseNullableGuid(row.activated_by),
            createdBy: ParseNullableGuid(row.created_by),
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool b ? b : Convert.ToBoolean(row.is_deleted),
            deletedAt: row.deleted_at == null || row.deleted_at is DBNull ? null : (DateTime?)row.deleted_at);
    }
}
