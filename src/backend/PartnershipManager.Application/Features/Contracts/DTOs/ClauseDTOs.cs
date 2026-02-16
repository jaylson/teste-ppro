using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Contracts.DTOs;

/// <summary>
/// Response DTO for Clause entity
/// </summary>
public record ClauseResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public ClauseType ClauseType { get; init; }
    public bool IsMandatory { get; init; }
    public int DisplayOrder { get; init; }
    public int Version { get; init; }
    public string Content { get; init; } = string.Empty;
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
/// Paged list response for Clause
/// </summary>
public class ClauseListResponse : PagedResult<ClauseResponse>
{
    public ClauseListResponse(
        IEnumerable<ClauseResponse> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        : base(items, totalCount, pageNumber, pageSize)
    {
    }
}

/// <summary>
/// Request DTO for creating a new Clause
/// </summary>
public record CreateClauseRequest
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public ClauseType ClauseType { get; init; }
    public bool IsMandatory { get; init; } = false;
    public int DisplayOrder { get; init; } = 0;
    public List<string> Tags { get; init; } = new();
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for updating an existing Clause
/// </summary>
public record UpdateClauseRequest
{
    public string Name { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsMandatory { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public List<string> Tags { get; init; } = new();
    public string? Description { get; init; }
}
