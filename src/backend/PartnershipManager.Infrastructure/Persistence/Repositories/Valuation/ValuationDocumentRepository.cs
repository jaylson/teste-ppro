using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class ValuationDocumentRepository : IValuationDocumentRepository
{
    private readonly DapperContext _context;

    public ValuationDocumentRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        d.id, d.valuation_id, d.client_id,
        d.file_name, d.file_size_bytes, d.mime_type,
        d.storage_path, d.download_url,
        d.is_verified, d.verified_at, d.verified_by,
        d.created_at, d.updated_at, d.created_by, d.updated_by, d.is_deleted, d.deleted_at";

    public async Task<IEnumerable<ValuationDocument>> GetByValuationAsync(Guid valuationId, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM valuation_documents d
            WHERE d.valuation_id = @ValuationId AND d.client_id = @ClientId AND d.is_deleted = 0
            ORDER BY d.created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ValuationId = valuationId.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToDocument);
    }

    public async Task<ValuationDocument?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM valuation_documents d
            WHERE d.id = @Id AND d.client_id = @ClientId AND d.is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToDocument(row);
    }

    public async Task AddAsync(ValuationDocument doc)
    {
        const string sql = @"
            INSERT INTO valuation_documents (
                id, valuation_id, client_id,
                file_name, file_size_bytes, mime_type,
                storage_path, download_url,
                is_verified, verified_at, verified_by,
                created_at, updated_at, created_by, updated_by, is_deleted
            ) VALUES (
                @Id, @ValuationId, @ClientId,
                @FileName, @FileSizeBytes, @MimeType,
                @StoragePath, @DownloadUrl,
                @IsVerified, @VerifiedAt, @VerifiedBy,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, 0
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(doc), _context.Transaction);
    }

    public async Task UpdateAsync(ValuationDocument doc)
    {
        const string sql = @"
            UPDATE valuation_documents SET
                download_url = @DownloadUrl,
                is_verified = @IsVerified, verified_at = @VerifiedAt, verified_by = @VerifiedBy,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(doc), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        const string sql = @"
            UPDATE valuation_documents
            SET is_deleted = 1, deleted_at = @Now, updated_at = @Now, updated_by = @DeletedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            Now = DateTime.UtcNow,
            DeletedBy = deletedBy?.ToString()
        }, _context.Transaction);
    }

    // ──── Mapping ─────────────────────────────────

    private static ValuationDocument MapToDocument(dynamic r)
    {
        var d = (ValuationDocument)Activator.CreateInstance(typeof(ValuationDocument), nonPublic: true)!;
        Set(d, "Id", Guid.Parse((string)r.id));
        Set(d, "ValuationId", Guid.Parse((string)r.valuation_id));
        Set(d, "ClientId", Guid.Parse((string)r.client_id));
        Set(d, "FileName", (string)r.file_name);
        Set(d, "FileSizeBytes", (long)r.file_size_bytes);
        Set(d, "MimeType", (string)r.mime_type);
        Set(d, "StoragePath", (string)r.storage_path);
        Set(d, "DownloadUrl", (string?)r.download_url);
        Set(d, "IsVerified", ((sbyte?)r.is_verified ?? 0) == 1);
        Set(d, "VerifiedAt", (DateTime?)r.verified_at);
        Set(d, "VerifiedBy", r.verified_by is null ? (Guid?)null : Guid.Parse((string)r.verified_by));
        Set(d, "CreatedAt", (DateTime)r.created_at);
        Set(d, "UpdatedAt", (DateTime)r.updated_at);
        Set(d, "CreatedBy", r.created_by is null ? (Guid?)null : Guid.Parse((string)r.created_by));
        Set(d, "UpdatedBy", r.updated_by is null ? (Guid?)null : Guid.Parse((string)r.updated_by));
        Set(d, "IsDeleted", r.is_deleted == 1);
        return d;
    }

    private static void Set(object obj, string prop, object? value)
    {
        var p = obj.GetType().GetProperty(prop,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        p?.SetValue(obj, value);
    }

    private static object ToParams(ValuationDocument doc) => new
    {
        Id = doc.Id.ToString(),
        ValuationId = doc.ValuationId.ToString(),
        ClientId = doc.ClientId.ToString(),
        doc.FileName,
        doc.FileSizeBytes,
        doc.MimeType,
        doc.StoragePath,
        doc.DownloadUrl,
        IsVerified = doc.IsVerified ? 1 : 0,
        doc.VerifiedAt,
        VerifiedBy = doc.VerifiedBy?.ToString(),
        doc.CreatedAt,
        doc.UpdatedAt,
        CreatedBy = doc.CreatedBy?.ToString(),
        UpdatedBy = doc.UpdatedBy?.ToString()
    };
}
