using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DapperContext _context;

    public DocumentRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        d.id, d.client_id, d.company_id,
        d.name, d.document_type, d.description,
        d.file_name, d.file_size_bytes, d.mime_type, d.storage_path, d.download_url,
        d.entity_type, d.entity_id,
        d.visibility, d.is_verified, d.verified_at, d.verified_by,
        d.created_at, d.updated_at, d.created_by, d.updated_by,
        d.is_deleted, d.deleted_at";

    public async Task<(IEnumerable<Document> Items, int Total)> GetPagedAsync(
        Guid clientId, Guid companyId, int page, int pageSize,
        string? documentType = null, string? visibility = null, string? search = null)
    {
        var where = "WHERE d.client_id = @ClientId AND d.company_id = @CompanyId AND d.is_deleted = 0";
        if (!string.IsNullOrWhiteSpace(documentType)) where += " AND d.document_type = @DocumentType";
        if (!string.IsNullOrWhiteSpace(visibility)) where += " AND d.visibility = @Visibility";
        if (!string.IsNullOrWhiteSpace(search)) where += " AND d.name LIKE @Search";

        var sql = $@"
            SELECT SQL_CALC_FOUND_ROWS {SelectColumns}
            FROM documents d
            {where}
            ORDER BY d.created_at DESC
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        using var multi = await _context.Connection.QueryMultipleAsync(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            DocumentType = documentType,
            Visibility = visibility,
            Search = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%",
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        }, _context.Transaction);

        var rows = await multi.ReadAsync<dynamic>();
        var total = await multi.ReadFirstAsync<int>();
        return (rows.Select(MapToDocument), total);
    }

    public async Task<IEnumerable<Document>> GetByEntityAsync(
        Guid clientId, Guid companyId, string entityType, Guid entityId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM documents d
            WHERE d.client_id = @ClientId AND d.company_id = @CompanyId
              AND d.entity_type = @EntityType AND d.entity_id = @EntityId
              AND d.is_deleted = 0
            ORDER BY d.created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            CompanyId = companyId.ToString(),
            EntityType = entityType,
            EntityId = entityId.ToString()
        }, _context.Transaction);

        return rows.Select(MapToDocument);
    }

    public async Task<Document?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM documents d
            WHERE d.id = @Id AND d.client_id = @ClientId AND d.is_deleted = 0";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, _context.Transaction);

        return row is null ? null : MapToDocument(row);
    }

    public async Task AddAsync(Document doc)
    {
        const string sql = @"
            INSERT INTO documents (
                id, client_id, company_id, name, document_type, description,
                file_name, file_size_bytes, mime_type, storage_path, download_url,
                entity_type, entity_id, visibility,
                is_verified, verified_at, verified_by,
                created_at, updated_at, created_by, updated_by, is_deleted
            ) VALUES (
                @Id, @ClientId, @CompanyId, @Name, @DocumentType, @Description,
                @FileName, @FileSizeBytes, @MimeType, @StoragePath, @DownloadUrl,
                @EntityType, @EntityId, @Visibility,
                @IsVerified, @VerifiedAt, @VerifiedBy,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, 0
            )";

        await _context.Connection.ExecuteAsync(sql, ToParams(doc), _context.Transaction);
    }

    public async Task UpdateAsync(Document doc)
    {
        const string sql = @"
            UPDATE documents SET
                name = @Name, description = @Description, visibility = @Visibility,
                download_url = @DownloadUrl,
                is_verified = @IsVerified, verified_at = @VerifiedAt, verified_by = @VerifiedBy,
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";

        await _context.Connection.ExecuteAsync(sql, ToParams(doc), _context.Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        const string sql = @"
            UPDATE documents
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

    private static Document MapToDocument(dynamic r)
    {
        var d = (Document)Activator.CreateInstance(typeof(Document), nonPublic: true)!;
        Set(d, "Id", Guid.Parse((string)r.id));
        Set(d, "ClientId", Guid.Parse((string)r.client_id));
        Set(d, "CompanyId", Guid.Parse((string)r.company_id));
        Set(d, "Name", (string)r.name);
        Set(d, "DocumentType", (string)r.document_type);
        Set(d, "Description", (string?)r.description);
        Set(d, "FileName", (string)r.file_name);
        Set(d, "FileSizeBytes", (long)r.file_size_bytes);
        Set(d, "MimeType", (string)r.mime_type);
        Set(d, "StoragePath", (string)r.storage_path);
        Set(d, "DownloadUrl", (string?)r.download_url);
        Set(d, "EntityType", (string?)r.entity_type);
        Set(d, "EntityId", r.entity_id is null ? (Guid?)null : Guid.Parse((string)r.entity_id));
        Set(d, "Visibility", (string)r.visibility);
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

    private static object ToParams(Document doc) => new
    {
        Id = doc.Id.ToString(),
        ClientId = doc.ClientId.ToString(),
        CompanyId = doc.CompanyId.ToString(),
        doc.Name,
        doc.DocumentType,
        doc.Description,
        doc.FileName,
        doc.FileSizeBytes,
        doc.MimeType,
        doc.StoragePath,
        doc.DownloadUrl,
        doc.EntityType,
        EntityId = doc.EntityId?.ToString(),
        doc.Visibility,
        IsVerified = doc.IsVerified ? 1 : 0,
        doc.VerifiedAt,
        VerifiedBy = doc.VerifiedBy?.ToString(),
        doc.CreatedAt,
        doc.UpdatedAt,
        CreatedBy = doc.CreatedBy?.ToString(),
        UpdatedBy = doc.UpdatedBy?.ToString()
    };
}
