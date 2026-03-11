namespace PartnershipManager.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = "both";
}

public class EmailLog : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ResendMessageId { get; set; }
    public string Status { get; set; } = "queued";
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}

public static class NotificationTypes
{
    public const string CommunicationPublished = "communication_published";
    public const string WorkflowAssigned = "workflow_assigned";
    public const string WorkflowApproved = "workflow_approved";
    public const string WorkflowRejected = "workflow_rejected";
    public const string ContractSigned = "contract_signed";
    public const string VestingEvent = "vesting_event";
    public const string DocumentUploaded = "document_uploaded";
    public const string System = "system";
}

public static class NotificationChannels
{
    public const string InApp = "in_app";
    public const string Email = "email";
    public const string Both = "both";
    public const string None = "none";
}
