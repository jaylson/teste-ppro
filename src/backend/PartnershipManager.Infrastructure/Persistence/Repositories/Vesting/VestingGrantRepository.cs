using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class VestingGrantRepository : IVestingGrantRepository
{
    private readonly DapperContext _context;

    public VestingGrantRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<VestingGrant> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid? companyId, int page, int pageSize,
        Guid? vestingPlanId = null, Guid? shareholderId = null, string? status = null)
    {
        var where = "WHERE vg.client_id = @ClientId AND vg.is_deleted = 0";
        if (companyId.HasValue) where += " AND vg.company_id = @CompanyId";
        if (vestingPlanId.HasValue) where += " AND vg.vesting_plan_id = @VestingPlanId";
        if (shareholderId.HasValue) where += " AND vg.shareholder_id = @ShareholderId";
        if (!string.IsNullOrWhiteSpace(status)) where += " AND vg.status = @Status";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                vg.id, vg.client_id, vg.vesting_plan_id, vg.shareholder_id, vg.company_id,
                vg.grant_date, vg.total_shares, vg.share_price, vg.equity_percentage,
                vg.vesting_start_date, vg.vesting_end_date, vg.cliff_date,
                vg.status, vg.vested_shares, vg.exercised_shares,
                vg.approved_at, vg.approved_by, vg.notes,
                vg.created_at, vg.updated_at, vg.is_deleted, vg.deleted_at
            FROM vesting_grants vg
            {where}
            ORDER BY vg.grant_date DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId?.ToString(),
            VestingPlanId = vestingPlanId?.ToString(),
            ShareholderId = shareholderId?.ToString(),
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToVestingGrant), total);
    }

    public async Task<VestingGrant?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = @"
            SELECT id, client_id, vesting_plan_id, shareholder_id, company_id,
                grant_date, total_shares, share_price, equity_percentage,
                vesting_start_date, vesting_end_date, cliff_date,
                status, vested_shares, exercised_shares,
                approved_at, approved_by, notes,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_grants
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToVestingGrant(row);
    }

    public async Task<IEnumerable<VestingGrant>> GetByShareholderAsync(Guid clientId, Guid shareholderId, Guid? companyId = null)
    {
        var where = "WHERE client_id = @ClientId AND shareholder_id = @ShareholderId AND is_deleted = 0";
        if (companyId.HasValue) where += " AND company_id = @CompanyId";

        var sql = $@"
            SELECT id, client_id, vesting_plan_id, shareholder_id, company_id,
                grant_date, total_shares, share_price, equity_percentage,
                vesting_start_date, vesting_end_date, cliff_date,
                status, vested_shares, exercised_shares,
                approved_at, approved_by, notes,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_grants
            {where}
            ORDER BY grant_date DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            ShareholderId = shareholderId.ToString(),
            CompanyId = companyId?.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVestingGrant);
    }

    public async Task<IEnumerable<VestingGrant>> GetByPlanAsync(Guid clientId, Guid vestingPlanId)
    {
        var sql = @"
            SELECT id, client_id, vesting_plan_id, shareholder_id, company_id,
                grant_date, total_shares, share_price, equity_percentage,
                vesting_start_date, vesting_end_date, cliff_date,
                status, vested_shares, exercised_shares,
                approved_at, approved_by, notes,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_grants
            WHERE client_id = @ClientId AND vesting_plan_id = @VestingPlanId AND is_deleted = 0
            ORDER BY grant_date DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            VestingPlanId = vestingPlanId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVestingGrant);
    }

    public async Task<IEnumerable<VestingGrant>> GetActiveGrantsForCompanyAsync(Guid clientId, Guid companyId)
    {
        var activeStatus = (int)VestingGrantDetailStatus.Active;
        var sql = @"
            SELECT id, client_id, vesting_plan_id, shareholder_id, company_id,
                grant_date, total_shares, share_price, equity_percentage,
                vesting_start_date, vesting_end_date, cliff_date,
                status, vested_shares, exercised_shares,
                approved_at, approved_by, notes,
                created_at, updated_at, is_deleted, deleted_at
            FROM vesting_grants
            WHERE client_id = @ClientId AND company_id = @CompanyId
              AND status = @Status AND is_deleted = 0
            ORDER BY grant_date DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            Status = activeStatus
        }, _context.Transaction);

        return rows.Select(MapToVestingGrant);
    }

    public async Task AddAsync(VestingGrant grant)
    {
        var sql = @"
            INSERT INTO vesting_grants
                (id, client_id, vesting_plan_id, shareholder_id, company_id,
                 grant_date, total_shares, share_price, equity_percentage,
                 vesting_start_date, vesting_end_date, cliff_date,
                 status, vested_shares, exercised_shares,
                 approved_at, approved_by, notes,
                 created_by, updated_by,
                 created_at, updated_at, is_deleted)
            VALUES
                (@Id, @ClientId, @VestingPlanId, @ShareholderId, @CompanyId,
                 @GrantDate, @TotalShares, @SharePrice, @EquityPercentage,
                 @VestingStartDate, @VestingEndDate, @CliffDate,
                 @Status, @VestedShares, @ExercisedShares,
                 @ApprovedAt, @ApprovedBy, @Notes,
                 @CreatedBy, @UpdatedBy,
                 @CreatedAt, @UpdatedAt, 0)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = grant.Id.ToString(),
            ClientId = grant.ClientId.ToString(),
            VestingPlanId = grant.VestingPlanId.ToString(),
            ShareholderId = grant.ShareholderId.ToString(),
            CompanyId = grant.CompanyId.ToString(),
            grant.GrantDate,
            grant.TotalShares,
            grant.SharePrice,
            grant.EquityPercentage,
            grant.VestingStartDate,
            grant.VestingEndDate,
            grant.CliffDate,
            Status = grant.Status.ToString(),
            grant.VestedShares,
            grant.ExercisedShares,
            grant.ApprovedAt,
            ApprovedBy = grant.ApprovedBy?.ToString(),
            grant.Notes,
            CreatedBy = grant.CreatedBy?.ToString(),
            UpdatedBy = grant.UpdatedBy?.ToString(),
            grant.CreatedAt,
            grant.UpdatedAt
        }, _context.Transaction);
    }

    public async Task UpdateAsync(VestingGrant grant)
    {
        var sql = @"
            UPDATE vesting_grants SET
                status = @Status,
                vested_shares = @VestedShares,
                exercised_shares = @ExercisedShares,
                approved_at = @ApprovedAt,
                approved_by = @ApprovedBy,
                notes = @Notes,
                updated_at = @UpdatedAt
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Status = grant.Status.ToString(),
            grant.VestedShares,
            grant.ExercisedShares,
            grant.ApprovedAt,
            ApprovedBy = grant.ApprovedBy?.ToString(),
            grant.Notes,
            grant.UpdatedAt,
            Id = grant.Id.ToString(),
            ClientId = grant.ClientId.ToString()
        }, _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"
            UPDATE vesting_grants
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
        var sql = "SELECT COUNT(1) FROM vesting_grants WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";
        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);
        return count > 0;
    }

    private static VestingGrant MapToVestingGrant(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null || value is DBNull ? null
            : (value is Guid g ? g : Guid.Parse(value.ToString()!));

        var status = row.status is int st ? (VestingGrantDetailStatus)st : Enum.Parse<VestingGrantDetailStatus>(row.status.ToString()!);

        return VestingGrant.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingPlanId: ParseGuid(row.vesting_plan_id),
            shareholderId: ParseGuid(row.shareholder_id),
            companyId: ParseGuid(row.company_id),
            grantDate: (DateTime)row.grant_date,
            totalShares: row.total_shares is decimal ts ? ts : Convert.ToDecimal(row.total_shares),
            sharePrice: row.share_price is decimal sp ? sp : Convert.ToDecimal(row.share_price),
            equityPercentage: row.equity_percentage is decimal ep ? ep : Convert.ToDecimal(row.equity_percentage),
            vestingStartDate: (DateTime)row.vesting_start_date,
            vestingEndDate: (DateTime)row.vesting_end_date,
            cliffDate: row.cliff_date == null || row.cliff_date is DBNull ? null : (DateTime?)row.cliff_date,
            status: status,
            vestedShares: row.vested_shares is decimal vs ? vs : Convert.ToDecimal(row.vested_shares),
            exercisedShares: row.exercised_shares is decimal es ? es : Convert.ToDecimal(row.exercised_shares),
            approvedAt: row.approved_at == null || row.approved_at is DBNull ? null : (DateTime?)row.approved_at,
            approvedBy: ParseNullableGuid(row.approved_by),
            notes: row.notes == null || row.notes is DBNull ? null : (string)row.notes,
            createdBy: ParseNullableGuid(row.created_by),
            createdAt: (DateTime)row.created_at,
            updatedAt: (DateTime)row.updated_at,
            isDeleted: row.is_deleted is bool b ? b : Convert.ToBoolean(row.is_deleted),
            deletedAt: row.deleted_at == null || row.deleted_at is DBNull ? null : (DateTime?)row.deleted_at);
    }
}
