using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Append-only ledger for vesting exercise transactions. Records are never updated or soft-deleted.
/// </summary>
public class VestingTransactionRepository : IVestingTransactionRepository
{
    private readonly DapperContext _context;

    public VestingTransactionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VestingTransaction>> GetByGrantAsync(Guid vestingGrantId)
    {
        var sql = @"
            SELECT id, client_id, vesting_grant_id, shareholder_id, company_id,
                transaction_date, shares_exercised, share_price_at_exercise, strike_price,
                share_transaction_id, transaction_type, notes,
                created_at, created_by
            FROM vesting_transactions
            WHERE vesting_grant_id = @VestingGrantId
            ORDER BY transaction_date DESC, created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            VestingGrantId = vestingGrantId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToVestingTransaction);
    }

    public async Task<IEnumerable<VestingTransaction>> GetByShareholderAsync(
        Guid clientId, Guid shareholderId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var where = "WHERE client_id = @ClientId AND shareholder_id = @ShareholderId";
        if (fromDate.HasValue) where += " AND transaction_date >= @FromDate";
        if (toDate.HasValue) where += " AND transaction_date <= @ToDate";

        var sql = $@"
            SELECT id, client_id, vesting_grant_id, shareholder_id, company_id,
                transaction_date, shares_exercised, share_price_at_exercise, strike_price,
                share_transaction_id, transaction_type, notes,
                created_at, created_by
            FROM vesting_transactions
            {where}
            ORDER BY transaction_date DESC, created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            ShareholderId = shareholderId.ToString(),
            FromDate = fromDate,
            ToDate = toDate
        }, _context.Transaction);

        return rows.Select(MapToVestingTransaction);
    }

    public async Task<(IEnumerable<VestingTransaction> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        Guid? shareholderId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var where = "WHERE client_id = @ClientId AND company_id = @CompanyId";
        if (shareholderId.HasValue) where += " AND shareholder_id = @ShareholderId";
        if (fromDate.HasValue) where += " AND transaction_date >= @FromDate";
        if (toDate.HasValue) where += " AND transaction_date <= @ToDate";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS
                id, client_id, vesting_grant_id, shareholder_id, company_id,
                transaction_date, shares_exercised, share_price_at_exercise, strike_price,
                share_transaction_id, transaction_type, notes,
                created_at, created_by
            FROM vesting_transactions
            {where}
            ORDER BY transaction_date DESC, created_at DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            ShareholderId = shareholderId?.ToString(),
            FromDate = fromDate,
            ToDate = toDate,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToVestingTransaction), total);
    }

    public async Task AddAsync(VestingTransaction transaction)
    {
        var sql = @"
            INSERT INTO vesting_transactions
                (id, client_id, vesting_grant_id, shareholder_id, company_id,
                 transaction_date, shares_exercised, share_price_at_exercise, strike_price,
                 share_transaction_id, transaction_type, notes,
                 created_at, created_by)
            VALUES
                (@Id, @ClientId, @VestingGrantId, @ShareholderId, @CompanyId,
                 @TransactionDate, @SharesExercised, @SharePriceAtExercise, @StrikePrice,
                 @ShareTransactionId, @TransactionType, @Notes,
                 @CreatedAt, @CreatedBy)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = transaction.Id.ToString(),
            ClientId = transaction.ClientId.ToString(),
            VestingGrantId = transaction.VestingGrantId.ToString(),
            ShareholderId = transaction.ShareholderId.ToString(),
            CompanyId = transaction.CompanyId.ToString(),
            transaction.TransactionDate,
            transaction.SharesExercised,
            transaction.SharePriceAtExercise,
            transaction.StrikePrice,
            ShareTransactionId = transaction.ShareTransactionId?.ToString(),
            TransactionType = (int)transaction.TransactionType,
            transaction.Notes,
            transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy.ToString()
        }, _context.Transaction);
    }

    public async Task UpdateShareTransactionLinkAsync(Guid id, Guid shareTransactionId)
    {
        var sql = @"
            UPDATE vesting_transactions
            SET share_transaction_id = @ShareTransactionId
            WHERE id = @Id AND share_transaction_id IS NULL";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ShareTransactionId = shareTransactionId.ToString()
        }, _context.Transaction);
    }

    private static VestingTransaction MapToVestingTransaction(dynamic row)
    {
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null || value is DBNull ? null
            : (value is Guid g ? g : Guid.Parse(value.ToString()!));

        var txType = row.transaction_type is int tt ? (VestingTransactionType)tt : Enum.Parse<VestingTransactionType>(row.transaction_type.ToString()!);

        return VestingTransaction.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            vestingGrantId: ParseGuid(row.vesting_grant_id),
            shareholderId: ParseGuid(row.shareholder_id),
            companyId: ParseGuid(row.company_id),
            transactionDate: (DateTime)row.transaction_date,
            sharesExercised: row.shares_exercised is decimal se ? se : Convert.ToDecimal(row.shares_exercised),
            sharePriceAtExercise: row.share_price_at_exercise is decimal spe ? spe : Convert.ToDecimal(row.share_price_at_exercise),
            strikePrice: row.strike_price is decimal sp ? sp : Convert.ToDecimal(row.strike_price),
            shareTransactionId: ParseNullableGuid(row.share_transaction_id),
            transactionType: txType,
            notes: row.notes == null || row.notes is DBNull ? null : (string)row.notes,
            createdAt: (DateTime)row.created_at,
            createdBy: ParseGuid(row.created_by));
    }
}
