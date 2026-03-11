using PartnershipManager.Application.DTOs.DataRoom;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Application.Services;

public interface IDataRoomService
{
    Task<DataRoomResponse> GetOrCreateDataRoomAsync(Guid companyId, Guid userId);
    Task<IEnumerable<DataRoomFolderResponse>> GetFoldersAsync(Guid companyId, Guid? parentId = null);
    Task<Guid> CreateFolderAsync(Guid companyId, CreateFolderRequest request, Guid userId);
    Task DeleteFolderAsync(Guid folderId);
    Task<IEnumerable<Document>> GetDocumentsInFolderAsync(Guid folderId);
    Task AddDocumentToFolderAsync(Guid folderId, Guid documentId, Guid userId);
    Task RemoveDocumentFromFolderAsync(Guid folderId, Guid documentId);
}

public class DataRoomService : IDataRoomService
{
    private readonly IDataRoomRepository _repo;

    public DataRoomService(IDataRoomRepository repo)
    {
        _repo = repo;
    }

    public async Task<DataRoomResponse> GetOrCreateDataRoomAsync(Guid companyId, Guid userId)
    {
        var dr = await _repo.GetByCompanyAsync(companyId);
        if (dr == null)
        {
            var newDr = new DataRoom { CompanyId = companyId, Name = "Data Room", IsActive = true };
            newDr.CreatedBy = userId;
            newDr.UpdatedBy = userId;
            var id = await _repo.CreateDataRoomAsync(newDr);
            dr = await _repo.GetByCompanyAsync(companyId)
                ?? new DataRoom { Id = id, CompanyId = companyId, Name = "Data Room", IsActive = true };
        }
        return new DataRoomResponse
        {
            Id = dr.Id,
            CompanyId = dr.CompanyId,
            Name = dr.Name,
            Description = dr.Description,
            IsActive = dr.IsActive,
            CreatedAt = dr.CreatedAt
        };
    }

    public async Task<IEnumerable<DataRoomFolderResponse>> GetFoldersAsync(Guid companyId, Guid? parentId = null)
    {
        var dr = await _repo.GetByCompanyAsync(companyId);
        if (dr == null) return Enumerable.Empty<DataRoomFolderResponse>();

        var folders = await _repo.GetFoldersAsync(dr.Id, parentId);
        return folders.Select(f => new DataRoomFolderResponse
        {
            Id = f.Id,
            DataRoomId = f.DataRoomId,
            ParentId = f.ParentId,
            Name = f.Name,
            Description = f.Description,
            DisplayOrder = f.DisplayOrder,
            Visibility = f.Visibility,
            CreatedAt = f.CreatedAt
        });
    }

    public async Task<Guid> CreateFolderAsync(Guid companyId, CreateFolderRequest request, Guid userId)
    {
        var dr = await GetOrCreateDataRoomAsync(companyId, userId);
        var folder = new DataRoomFolder
        {
            DataRoomId = dr.Id,
            ParentId = request.ParentId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Visibility = request.Visibility
        };
        folder.CreatedBy = userId;
        folder.UpdatedBy = userId;
        return await _repo.CreateFolderAsync(folder);
    }

    public Task DeleteFolderAsync(Guid folderId)
        => _repo.SoftDeleteFolderAsync(folderId);

    public Task<IEnumerable<Document>> GetDocumentsInFolderAsync(Guid folderId)
        => _repo.GetDocumentsInFolderAsync(folderId);

    public Task AddDocumentToFolderAsync(Guid folderId, Guid documentId, Guid userId)
        => _repo.AddDocumentToFolderAsync(folderId, documentId, userId);

    public Task RemoveDocumentFromFolderAsync(Guid folderId, Guid documentId)
        => _repo.RemoveDocumentFromFolderAsync(folderId, documentId);
}
