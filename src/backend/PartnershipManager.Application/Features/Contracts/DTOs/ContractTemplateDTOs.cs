using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Contracts.DTOs;

/// <summary>
/// Response DTO for ContractTemplate entity
/// </summary>
public record ContractTemplateResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public ContractTemplateType TemplateType { get; init; }
    public string Content { get; init; } = string.Empty;
    public ContractStatus DefaultStatus { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? Description { get; init; }
    public List<string> Variables { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
}

/// <summary>
/// Paged list response for ContractTemplate
/// </summary>
public class ContractTemplateListResponse : PagedResult<ContractTemplateResponse>
{
    public ContractTemplateListResponse(
        IEnumerable<ContractTemplateResponse> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        : base(items, totalCount, pageNumber, pageSize)
    {
    }
}

/// <summary>
/// Request DTO for creating a new ContractTemplate
/// </summary>
public record CreateContractTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public ContractTemplateType TemplateType { get; init; }
    public string Content { get; init; } = string.Empty;
    public ContractStatus DefaultStatus { get; init; } = ContractStatus.Draft;
    public List<string> Tags { get; init; } = new();
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for updating an existing ContractTemplate
/// </summary>
public record UpdateContractTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public ContractStatus DefaultStatus { get; init; }
    public bool IsActive { get; init; } = true;
    public List<string> Tags { get; init; } = new();
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for cloning an existing template with a new code
/// </summary>
public record CloneContractTemplateRequest
{
    public string NewCode { get; init; } = string.Empty;
    public string? NewName { get; init; }
}
