namespace PartnershipManager.Domain.Entities;

public class DataRoom : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<DataRoomFolder> Folders { get; set; } = new List<DataRoomFolder>();
}

public class DataRoomFolder : BaseEntity
{
    public Guid DataRoomId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string Visibility { get; set; } = "internal";
    public ICollection<DataRoomFolder> SubFolders { get; set; } = new List<DataRoomFolder>();
}

public class DataRoomDocument : BaseEntity
{
    public Guid FolderId { get; set; }
    public Guid DocumentId { get; set; }
    public int DisplayOrder { get; set; }
    public Guid AddedBy { get; set; }
    public DateTime AddedAt { get; set; }
}
