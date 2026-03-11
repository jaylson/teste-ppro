namespace PartnershipManager.Application.DTOs.DataRoom;

public class DataRoomResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DataRoomFolderResponse
{
    public Guid Id { get; set; }
    public Guid DataRoomId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateFolderRequest
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string Visibility { get; set; } = "internal";
}

public class AddDocumentToFolderRequest
{
    public Guid DocumentId { get; set; }
}
