namespace PartnershipManager.Application.DTOs.Communication;

public class CreateCommunicationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    public string? Summary { get; set; }
    public string CommType { get; set; } = "announcement";
    public string Visibility { get; set; } = "all";
    public string? TargetRoles { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateCommunicationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    public string? Summary { get; set; }
    public string CommType { get; set; } = "announcement";
    public string Visibility { get; set; } = "all";
    public string? TargetRoles { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CommunicationResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    public string? Summary { get; set; }
    public string CommType { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string? TargetRoles { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewsCount { get; set; }
    public bool HasViewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CommunicationListResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CommType { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ViewsCount { get; set; }
    public string? Summary { get; set; }
}

public class TrackViewRequest
{
    public int? ViewDurationSecs { get; set; }
}
