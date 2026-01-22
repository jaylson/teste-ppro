namespace PartnershipManager.Application.Features.Billing.DTOs;

public record SubscriptionListResponseDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public decimal PlanPrice { get; init; }
    public string BillingCycle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool AutoRenew { get; init; }
    public int CompaniesCount { get; init; }
    public int UsersCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SubscriptionResponseDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ClientEmail { get; init; } = string.Empty;
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public decimal PlanPrice { get; init; }
    public string BillingCycle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool AutoRenew { get; init; }
    public int CompaniesCount { get; init; }
    public int UsersCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record SubscriptionCreateDto
{
    public Guid ClientId { get; init; }
    public Guid PlanId { get; init; }
    public DateTime? StartDate { get; init; }
    public bool AutoRenew { get; init; } = true;
}

public record SubscriptionUpdateDto
{
    public Guid PlanId { get; init; }
    public bool AutoRenew { get; init; }
    public int CompaniesCount { get; init; }
    public int UsersCount { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
