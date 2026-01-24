using System.Data;
using System.Text;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class ShareTransactionRepository : IShareTransactionRepository
{
    private readonly DapperContext _context;

    public ShareTransactionRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        t.id AS Id,
        t.client_id AS ClientId,
        t.company_id AS CompanyId,
        t.transaction_type AS TransactionType,
        t.transaction_number AS TransactionNumber,
        t.reference_date AS ReferenceDate,
        t.share_id AS ShareId,
        t.share_class_id AS ShareClassId,
        t.quantity AS Quantity,
        t.price_per_share AS PricePerShare,
        t.from_shareholder_id AS FromShareholderId,
        t.to_shareholder_id AS ToShareholderId,
        t.reason AS Reason,
        t.document_reference AS DocumentReference,
        t.notes AS Notes,
        t.approved_by AS ApprovedBy,
        t.approved_at AS ApprovedAt,
        t.created_at AS CreatedAt,
        t.created_by AS CreatedBy,
        fsh.name AS FromShareholderName,
        tsh.name AS ToShareholderName,
        sc.name AS ShareClassName,
        sc.code AS ShareClassCode,
        c.name AS CompanyName,
        u.name AS ApprovedByName";

    public async Task<(IEnumerable<ShareTransaction> Items, int Total, decimal TotalQuantity, decimal TotalValue)> GetPagedAsync(
        Guid clientId,
        Guid? companyId,
        int page,
        int pageSize,
        string? transactionType = null,
        Guid? shareholderId = null,
        Guid? shareClassId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var baseQuery = new StringBuilder(@"FROM share_transactions t
                    JOIN share_classes sc ON sc.id = t.share_class_id
                    JOIN companies c ON c.id = t.company_id
                    LEFT JOIN shareholders fsh ON fsh.id = t.from_shareholder_id
                    LEFT JOIN shareholders tsh ON tsh.id = t.to_shareholder_id
                    LEFT JOIN users u ON u.id = t.approved_by
                    WHERE t.client_id = @ClientId");

        var parameters = new DynamicParameters();
        parameters.Add("ClientId", clientId.ToString());

        if (companyId.HasValue)
        {
            baseQuery.Append(" AND t.company_id = @CompanyId");
            parameters.Add("CompanyId", companyId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            baseQuery.Append(" AND t.transaction_type = @TransactionType");
            parameters.Add("TransactionType", transactionType);
        }

        if (shareholderId.HasValue)
        {
            baseQuery.Append(" AND (t.from_shareholder_id = @ShareholderId OR t.to_shareholder_id = @ShareholderId)");
            parameters.Add("ShareholderId", shareholderId.Value.ToString());
        }

        if (shareClassId.HasValue)
        {
            baseQuery.Append(" AND t.share_class_id = @ShareClassId");
            parameters.Add("ShareClassId", shareClassId.Value.ToString());
        }

        if (fromDate.HasValue)
        {
            baseQuery.Append(" AND t.reference_date >= @FromDate");
            parameters.Add("FromDate", fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            baseQuery.Append(" AND t.reference_date <= @ToDate");
            parameters.Add("ToDate", toDate.Value.Date);
        }

        // Count and aggregates
        var countSql = $@"SELECT COUNT(1) {baseQuery}";
        var total = await Connection.ExecuteScalarAsync<int>(countSql, parameters, Transaction);

        var aggregateSql = $@"SELECT COALESCE(SUM(t.quantity), 0) AS TotalQuantity, 
                               COALESCE(SUM(t.quantity * t.price_per_share), 0) AS TotalValue 
                               {baseQuery}";
        var aggregates = await Connection.QueryFirstOrDefaultAsync<dynamic>(aggregateSql, parameters, Transaction);
        var totalQuantity = (decimal)(aggregates?.TotalQuantity ?? 0m);
        var totalValue = (decimal)(aggregates?.TotalValue ?? 0m);

        // Paged results
        baseQuery.Append(" ORDER BY t.reference_date DESC, t.created_at DESC LIMIT @Offset, @PageSize");
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var sql = $@"SELECT {SelectColumns} {baseQuery}";

        var rows = await Connection.QueryAsync<dynamic>(sql, parameters, Transaction);
        var items = rows.Select(MapToTransaction);

        return (items, total, totalQuantity, totalValue);
    }

    public async Task<IEnumerable<ShareTransaction>> GetByShareAsync(Guid clientId, Guid shareId)
    {
        var sql = $@"SELECT {SelectColumns}
                    FROM share_transactions t
                    JOIN share_classes sc ON sc.id = t.share_class_id
                    JOIN companies c ON c.id = t.company_id
                    LEFT JOIN shareholders fsh ON fsh.id = t.from_shareholder_id
                    LEFT JOIN shareholders tsh ON tsh.id = t.to_shareholder_id
                    LEFT JOIN users u ON u.id = t.approved_by
                    WHERE t.client_id = @ClientId
                      AND t.share_id = @ShareId
                    ORDER BY t.created_at ASC";

        var rows = await Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            ShareId = shareId.ToString()
        }, Transaction);

        return rows.Select(MapToTransaction);
    }

    public async Task<IEnumerable<ShareTransaction>> GetByShareholderAsync(Guid clientId, Guid shareholderId)
    {
        var sql = $@"SELECT {SelectColumns}
                    FROM share_transactions t
                    JOIN share_classes sc ON sc.id = t.share_class_id
                    JOIN companies c ON c.id = t.company_id
                    LEFT JOIN shareholders fsh ON fsh.id = t.from_shareholder_id
                    LEFT JOIN shareholders tsh ON tsh.id = t.to_shareholder_id
                    LEFT JOIN users u ON u.id = t.approved_by
                    WHERE t.client_id = @ClientId
                      AND (t.from_shareholder_id = @ShareholderId OR t.to_shareholder_id = @ShareholderId)
                    ORDER BY t.reference_date DESC, t.created_at DESC";

        var rows = await Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            ShareholderId = shareholderId.ToString()
        }, Transaction);

        return rows.Select(MapToTransaction);
    }

    public async Task<ShareTransaction?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"SELECT {SelectColumns}
                    FROM share_transactions t
                    JOIN share_classes sc ON sc.id = t.share_class_id
                    JOIN companies c ON c.id = t.company_id
                    LEFT JOIN shareholders fsh ON fsh.id = t.from_shareholder_id
                    LEFT JOIN shareholders tsh ON tsh.id = t.to_shareholder_id
                    LEFT JOIN users u ON u.id = t.approved_by
                    WHERE t.id = @Id AND t.client_id = @ClientId";

        var transaction = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return transaction == null ? null : MapToTransaction(transaction);
    }

    public async Task<string> GetNextTransactionNumberAsync(Guid companyId)
    {
        var sql = @"SELECT COUNT(1) + 1 FROM share_transactions WHERE company_id = @CompanyId";
        var count = await Connection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId.ToString() }, Transaction);
        return $"TXN-{DateTime.UtcNow:yyyy}-{count:D4}";
    }

    public async Task AddAsync(ShareTransaction transaction)
    {
        var sql = @"INSERT INTO share_transactions
                    (id, client_id, company_id, transaction_type, transaction_number, reference_date,
                     share_id, share_class_id, quantity, price_per_share, from_shareholder_id, to_shareholder_id,
                     reason, document_reference, notes, approved_by, approved_at, created_at, created_by)
                    VALUES
                    (@Id, @ClientId, @CompanyId, @TransactionType, @TransactionNumber, @ReferenceDate,
                     @ShareId, @ShareClassId, @Quantity, @PricePerShare, @FromShareholderId, @ToShareholderId,
                     @Reason, @DocumentReference, @Notes, @ApprovedBy, @ApprovedAt, @CreatedAt, @CreatedBy)";

        await Connection.ExecuteAsync(sql, new
        {
            Id = transaction.Id.ToString(),
            ClientId = transaction.ClientId.ToString(),
            CompanyId = transaction.CompanyId.ToString(),
            TransactionType = transaction.TransactionType.ToString(),
            transaction.TransactionNumber,
            transaction.ReferenceDate,
            ShareId = transaction.ShareId?.ToString(),
            ShareClassId = transaction.ShareClassId.ToString(),
            transaction.Quantity,
            transaction.PricePerShare,
            FromShareholderId = transaction.FromShareholderId?.ToString(),
            ToShareholderId = transaction.ToShareholderId?.ToString(),
            transaction.Reason,
            transaction.DocumentReference,
            transaction.Notes,
            ApprovedBy = transaction.ApprovedBy?.ToString(),
            transaction.ApprovedAt,
            transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy?.ToString()
        }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid clientId)
    {
        var sql = @"SELECT COUNT(1) FROM share_transactions 
                    WHERE id = @Id AND client_id = @ClientId";

        var count = await Connection.ExecuteScalarAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return count > 0;
    }

    private static ShareTransaction MapToTransaction(dynamic row)
    {
        var transactionType = Enum.Parse<TransactionType>(row.TransactionType?.ToString() ?? "Issue");

        // Create the appropriate transaction type
        ShareTransaction transaction = transactionType switch
        {
            TransactionType.Issue => ShareTransaction.CreateIssuance(
                ParseGuid(row.ClientId),
                ParseGuid(row.CompanyId),
                ParseGuid(row.ShareClassId),
                ParseGuid(row.ToShareholderId),
                row.Quantity,
                row.PricePerShare,
                row.ReferenceDate,
                row.TransactionNumber,
                row.Reason,
                row.DocumentReference,
                row.Notes,
                row.ApprovedBy != null ? ParseGuid(row.ApprovedBy) : null,
                row.CreatedBy != null ? ParseGuid(row.CreatedBy) : null
            ),
            TransactionType.Transfer => ShareTransaction.CreateTransfer(
                ParseGuid(row.ClientId),
                ParseGuid(row.CompanyId),
                ParseGuid(row.ShareClassId),
                ParseGuid(row.FromShareholderId),
                ParseGuid(row.ToShareholderId),
                row.Quantity,
                row.PricePerShare,
                row.ReferenceDate,
                row.ShareId != null ? ParseGuid(row.ShareId) : null,
                row.TransactionNumber,
                row.Reason,
                row.DocumentReference,
                row.Notes,
                row.ApprovedBy != null ? ParseGuid(row.ApprovedBy) : null,
                row.CreatedBy != null ? ParseGuid(row.CreatedBy) : null
            ),
            TransactionType.Cancel => ShareTransaction.CreateCancellation(
                ParseGuid(row.ClientId),
                ParseGuid(row.CompanyId),
                ParseGuid(row.ShareClassId),
                ParseGuid(row.FromShareholderId),
                row.Quantity,
                row.PricePerShare,
                row.ReferenceDate,
                row.ShareId != null ? ParseGuid(row.ShareId) : null,
                row.TransactionNumber,
                row.Reason ?? "Cancellation",
                row.DocumentReference,
                row.Notes,
                row.ApprovedBy != null ? ParseGuid(row.ApprovedBy) : null,
                row.CreatedBy != null ? ParseGuid(row.CreatedBy) : null
            ),
            _ => ShareTransaction.CreateIssuance(
                ParseGuid(row.ClientId),
                ParseGuid(row.CompanyId),
                ParseGuid(row.ShareClassId),
                row.ToShareholderId != null ? ParseGuid(row.ToShareholderId) : Guid.Empty,
                row.Quantity,
                row.PricePerShare,
                row.ReferenceDate,
                row.TransactionNumber,
                row.Reason,
                row.DocumentReference,
                row.Notes,
                row.ApprovedBy != null ? ParseGuid(row.ApprovedBy) : null,
                row.CreatedBy != null ? ParseGuid(row.CreatedBy) : null
            )
        };

        // Override the Id and ShareId from database
        SetProperty(transaction, "Id", ParseGuid(row.Id));
        if (row.ShareId != null)
        {
            SetProperty(transaction, "ShareId", ParseGuid(row.ShareId));
        }
        SetProperty(transaction, "CreatedAt", row.CreatedAt);
        
        // Navigation properties
        SetProperty(transaction, "FromShareholderName", row.FromShareholderName);
        SetProperty(transaction, "ToShareholderName", row.ToShareholderName);
        SetProperty(transaction, "ShareClassName", row.ShareClassName);
        SetProperty(transaction, "ShareClassCode", row.ShareClassCode);
        SetProperty(transaction, "CompanyName", row.CompanyName);
        SetProperty(transaction, "ApprovedByName", row.ApprovedByName);

        return transaction;
    }

    private static Guid ParseGuid(object value)
    {
        if (value is Guid guid) return guid;
        if (value is string str && Guid.TryParse(str, out var parsed)) return parsed;
        return Guid.Empty;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}
