namespace PartnershipManager.Application.DTOs.Notification;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationPreferenceResponse
{
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
}

public class UpdatePreferenceRequest
{
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = "both";
}
