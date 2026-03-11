using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories;

public class DataRoomRepository : IDataRoomRepository
{
    private readonly DapperContext _context;

    public DataRoomRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<DataRoom?> GetByCompanyAsync(Guid companyId)
    {
        var sql = @"
            SELECT id AS Id, company_id AS CompanyId, name AS Name, description AS Description,
                   is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM data_rooms
            WHERE company_id = @CompanyId AND deleted_at IS NULL
            LIMIT 1";
        return await _context.Connection.QueryFirstOrDefaultAsync<DataRoom>(sql, new { CompanyId = companyId });
    }

    public async Task<Guid> CreateDataRoomAsync(DataRoom dataRoom)
    {
        var sql = @"
            INSERT INTO data_rooms (id, company_id, name, description, is_active, created_at, updated_at, created_by, updated_by)
            VALUES (@Id, @CompanyId, @Name, @Description, @IsActive, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy)";
        await _context.Connection.ExecuteAsync(sql, dataRoom);
        return dataRoom.Id;
    }

    public async Task<IEnumerable<DataRoomFolder>> GetFoldersAsync(Guid dataRoomId, Guid? parentId = null)
    {
        var sql = parentId.HasValue
            ? @"SELECT id AS Id, data_room_id AS DataRoomId, parent_id AS ParentId, name AS Name,
                       description AS Description, display_order AS DisplayOrder, visibility AS Visibility,
                       created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
                FROM data_room_folders WHERE data_room_id = @DataRoomId AND parent_id = @ParentId AND deleted_at IS NULL ORDER BY display_order"
            : @"SELECT id AS Id, data_room_id AS DataRoomId, parent_id AS ParentId, name AS Name,
                       description AS Description, display_order AS DisplayOrder, visibility AS Visibility,
                       created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
                FROM data_room_folders WHERE data_room_id = @DataRoomId AND parent_id IS NULL AND deleted_at IS NULL ORDER BY display_order";

        return await _context.Connection.QueryAsync<DataRoomFolder>(sql, new { DataRoomId = dataRoomId, ParentId = parentId });
    }

    public async Task<DataRoomFolder?> GetFolderByIdAsync(Guid folderId)
    {
        var sql = @"SELECT id AS Id, data_room_id AS DataRoomId, parent_id AS ParentId, name AS Name,
                           description AS Description, display_order AS DisplayOrder, visibility AS Visibility,
                           created_by AS CreatedBy, created_at AS CreatedAt, updated_at AS UpdatedAt
                    FROM data_room_folders WHERE id = @Id AND deleted_at IS NULL";
        return await _context.Connection.QueryFirstOrDefaultAsync<DataRoomFolder>(sql, new { Id = folderId });
    }

    public async Task<Guid> CreateFolderAsync(DataRoomFolder folder)
    {
        var sql = @"
            INSERT INTO data_room_folders (id, data_room_id, parent_id, name, description, display_order, visibility, created_by, updated_by, created_at, updated_at)
            VALUES (@Id, @DataRoomId, @ParentId, @Name, @Description, @DisplayOrder, @Visibility, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await _context.Connection.ExecuteAsync(sql, folder);
        return folder.Id;
    }

    public async Task UpdateFolderAsync(DataRoomFolder folder)
    {
        folder.UpdatedAt = DateTime.UtcNow;
        var sql = @"
            UPDATE data_room_folders
            SET name = @Name, description = @Description, display_order = @DisplayOrder,
                visibility = @Visibility, updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, folder);
    }

    public async Task SoftDeleteFolderAsync(Guid folderId)
    {
        var sql = @"UPDATE data_room_folders SET deleted_at = @Now, updated_at = @Now WHERE id = @Id AND deleted_at IS NULL";
        await _context.Connection.ExecuteAsync(sql, new { Id = folderId, Now = DateTime.UtcNow });
    }

    public async Task<IEnumerable<Document>> GetDocumentsInFolderAsync(Guid folderId)
    {
        var sql = @"
            SELECT d.id AS Id, d.client_id AS ClientId, d.company_id AS CompanyId, d.name AS Name,
                   d.document_type AS DocumentType, d.description AS Description,
                   d.file_name AS FileName, d.file_size_bytes AS FileSizeBytes, d.mime_type AS MimeType,
                   d.storage_path AS StoragePath, d.download_url AS DownloadUrl,
                   d.entity_type AS EntityType, d.entity_id AS EntityId, d.visibility AS Visibility,
                   d.is_verified AS IsVerified, d.verified_at AS VerifiedAt,
                   d.created_at AS CreatedAt, d.updated_at AS UpdatedAt
            FROM documents d
            INNER JOIN data_room_folder_documents dfd ON dfd.document_id = d.id
            WHERE dfd.folder_id = @FolderId AND d.deleted_at IS NULL
            ORDER BY dfd.display_order";
        return await _context.Connection.QueryAsync<Document>(sql, new { FolderId = folderId });
    }

    public async Task AddDocumentToFolderAsync(Guid folderId, Guid documentId, Guid addedBy)
    {
        var maxOrder = await _context.Connection.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(display_order), 0) FROM data_room_folder_documents WHERE folder_id = @FolderId",
            new { FolderId = folderId });

        var sql = @"
            INSERT IGNORE INTO data_room_folder_documents (id, folder_id, document_id, display_order, added_by, added_at, created_at, updated_at)
            VALUES (@Id, @FolderId, @DocumentId, @Order, @AddedBy, @Now, @Now, @Now)";
        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(), FolderId = folderId, DocumentId = documentId,
            Order = maxOrder + 1, AddedBy = addedBy, Now = DateTime.UtcNow
        });
    }

    public async Task RemoveDocumentFromFolderAsync(Guid folderId, Guid documentId)
    {
        var sql = "DELETE FROM data_room_folder_documents WHERE folder_id = @FolderId AND document_id = @DocumentId";
        await _context.Connection.ExecuteAsync(sql, new { FolderId = folderId, DocumentId = documentId });
    }
}
