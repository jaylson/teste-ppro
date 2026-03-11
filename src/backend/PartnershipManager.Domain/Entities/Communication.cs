namespace PartnershipManager.Domain.Entities;

public class Communication : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    public string? Summary { get; set; }
    public string CommType { get; set; } = "announcement";
    public string Visibility { get; set; } = "all";
    public string? TargetRoles { get; set; }
    public string? Attachments { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewsCount { get; set; }
}

public class CommunicationView : BaseEntity
{
    public Guid CommunicationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ViewedAt { get; set; }
    public int? ViewDurationSecs { get; set; }
}

public static class CommunicationTypes
{
    public const string Announcement = "announcement";
    public const string Update = "update";
    public const string Report = "report";
    public const string Alert = "alert";
    public const string Invitation = "invitation";
    public static readonly IReadOnlySet<string> All = new HashSet<string> { Announcement, Update, Report, Alert, Invitation };
}

public static class CommunicationVisibilities
{
    public const string All = "all";
    public const string Investors = "investors";
    public const string Founders = "founders";
    public const string Employees = "employees";
    public const string Specific = "specific";
    public static readonly IReadOnlySet<string> AllValues = new HashSet<string> { All, Investors, Founders, Employees, Specific };
}
