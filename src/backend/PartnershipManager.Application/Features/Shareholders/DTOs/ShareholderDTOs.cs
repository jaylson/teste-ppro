using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Shareholders.DTOs;

public record ShareholderResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string DocumentFormatted { get; init; } = string.Empty;
    public DocumentType DocumentType { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public ShareholderType Type { get; init; }
    public ShareholderStatus Status { get; init; }
    public string? Notes { get; init; }
    public string? AddressStreet { get; init; }
    public string? AddressNumber { get; init; }
    public string? AddressComplement { get; init; }
    public string? AddressZipCode { get; init; }
    public string? AddressCity { get; init; }
    public string? AddressState { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }
    public Gender? Gender { get; init; }
    public DateTime? BirthDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class ShareholderListResponse : PagedResult<ShareholderResponse>
{
    public ShareholderListResponse(IEnumerable<ShareholderResponse> items, int totalCount, int pageNumber, int pageSize)
        : base(items, totalCount, pageNumber, pageSize)
    {
    }
}

public record CreateShareholderRequest
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public DocumentType DocumentType { get; init; }
    public ShareholderType Type { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
    public string? AddressStreet { get; init; }
    public string? AddressNumber { get; init; }
    public string? AddressComplement { get; init; }
    public string? AddressZipCode { get; init; }
    public string? AddressCity { get; init; }
    public string? AddressState { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }
    public Gender? Gender { get; init; }
    public DateTime? BirthDate { get; init; }
}

public record UpdateShareholderRequest
{
    public Guid? CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public ShareholderType Type { get; init; }
    public ShareholderStatus Status { get; init; }
    public string? Document { get; init; }
    public DocumentType? DocumentType { get; init; }
    public string? Notes { get; init; }
    public string? AddressStreet { get; init; }
    public string? AddressNumber { get; init; }
    public string? AddressComplement { get; init; }
    public string? AddressZipCode { get; init; }
    public string? AddressCity { get; init; }
    public string? AddressState { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }
    public Gender? Gender { get; init; }
    public DateTime? BirthDate { get; init; }
}
