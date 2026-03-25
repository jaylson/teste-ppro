namespace PartnershipManager.Domain.Entities;

public class Workflow : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";
    public string Priority { get; set; } = "medium";
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 1;
    public Guid RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }
    public string? Metadata { get; set; }
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
}

public class WorkflowStep : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StepType { get; set; } = "approval";
    public string? AssignedRole { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string Status { get; set; } = "pending";
    public bool IsCurrent { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedBy { get; set; }
    public string? Notes { get; set; }
    public ICollection<WorkflowApproval> Approvals { get; set; } = new List<WorkflowApproval>();
}

public class WorkflowApproval : BaseEntity
{
    public Guid WorkflowStepId { get; set; }
    public Guid UserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime DecidedAt { get; set; }
    public string? IpAddress { get; set; }
}

public static class WorkflowStatuses
{
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Cancelled = "cancelled";
}

public static class WorkflowTypes
{
    public const string ContractApproval = "contract_approval";
    public const string ShareholderChange = "shareholder_change";
    public const string CommunicationApproval = "communication_approval";
    public const string DocumentVerification = "document_verification";
    public const string VestingApproval = "vesting_approval";
}

public static class WorkflowStepStatuses
{
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Skipped = "skipped";
}

public static class WorkflowDecisions
{
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string RequestedChanges = "requested_changes";
}
