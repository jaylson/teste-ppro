using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Contracts.DTOs;

/// <summary>
/// Response DTO for ContractParty entity
/// </summary>
public record ContractPartyResponse
{
    public Guid Id { get; init; }
    public Guid ContractId { get; init; }
    public string PartyType { get; init; } = string.Empty;
    public string PartyName { get; init; } = string.Empty;
    public string PartyEmail { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public Guid? ShareholderId { get; init; }
    public SignatureStatus SignatureStatus { get; init; }
    public DateTime? SignatureDate { get; init; }
    public string? SignatureToken { get; init; }
    public string? SignatureExternalId { get; init; }
    public int SequenceOrder { get; init; }
    public string? RejectionReason { get; init; }
}

/// <summary>
/// Response DTO for ContractClause entity
/// </summary>
public record ContractClauseResponse
{
    public Guid Id { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClauseId { get; init; }
    public string? CustomContent { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsMandatory { get; init; }
    public Dictionary<string, string> ClauseVariables { get; init; } = new();
    public string? Notes { get; init; }
    
    // Navigation - Clause details
    public string ClauseName { get; init; } = string.Empty;
    public string ClauseCode { get; init; } = string.Empty;
    public ClauseType ClauseType { get; init; }
    public string EffectiveContent { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO for Contract entity
/// </summary>
public record ContractResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public ContractTemplateType ContractType { get; init; }
    public ContractStatus Status { get; init; }
    public Guid? TemplateId { get; init; }
    public string? TemplateName { get; init; }
    public string? DocumentPath { get; init; }
    public long? DocumentSize { get; init; }
    public string? DocumentHash { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
    
    // Navigation properties
    public List<ContractPartyResponse> Parties { get; init; } = new();
    public List<ContractClauseResponse> Clauses { get; init; } = new();
}

/// <summary>
/// Paged list response for Contract
/// </summary>
public class ContractListResponse : PagedResult<ContractResponse>
{
    public ContractListResponse(
        IEnumerable<ContractResponse> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        : base(items, totalCount, pageNumber, pageSize)
    {
    }
}

/// <summary>
/// Request DTO for creating a new Contract
/// </summary>
public record CreateContractRequest
{
    public Guid CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public ContractTemplateType ContractType { get; init; }
    public Guid? TemplateId { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for updating an existing Contract
/// </summary>
public record UpdateContractRequest
{
    public string Title { get; init; } = string.Empty;
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for adding a party to a contract
/// </summary>
public record AddContractPartyRequest
{
    public string PartyType { get; init; } = "signer"; // signer, recipient, witness
    public string PartyName { get; init; } = string.Empty;
    public string PartyEmail { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public Guid? ShareholderId { get; init; }
    public int SequenceOrder { get; init; } = 1;
}

/// <summary>
/// Request DTO for updating a party in a contract
/// </summary>
public record UpdateContractPartyRequest
{
    public string? PartyName { get; init; }
    public string? PartyEmail { get; init; }
    public int? SequenceOrder { get; init; }
}

/// <summary>
/// Request DVO for adding a clause to a contract
/// </summary>
public record AddContractClauseRequest
{
    public Guid ClauseId { get; init; }
    public string? CustomContent { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsMandatory { get; init; } = false;
    public Dictionary<string, string> ClauseVariables { get; init; } = new();
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for updating a clause in a contract
/// </summary>
public record UpdateContractClauseRequest
{
    public int? DisplayOrder { get; init; }
    public string? CustomContent { get; init; }
    public Dictionary<string, string>? ClauseVariables { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for reordering clauses in a contract
/// </summary>
public record ReorderClausesRequest
{
    public List<ClauseOrderItem> ClauseOrders { get; init; } = new();
}

/// <summary>
/// Item for clause ordering
/// </summary>
public record ClauseOrderItem
{
    public Guid ClauseId { get; init; }
    public int DisplayOrder { get; init; }
}

/// <summary>
/// Request DTO for updating contract status
/// </summary>
public record UpdateContractStatusRequest
{
    public ContractStatus Status { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// Request DTO for attaching a document to a contract
/// </summary>
public record AttachDocumentRequest
{
    public string DocumentPath { get; init; } = string.Empty;
    public long DocumentSize { get; init; }
    public string DocumentHash { get; init; } = string.Empty;
}

// =====================================================================
// VERSION HISTORY DTOs (Fase 4)
// =====================================================================

/// <summary>
/// Response DTO for a single contract document version
/// </summary>
public record ContractVersionResponse
{
    public Guid Id { get; init; }
    public Guid ContractId { get; init; }
    public int VersionNumber { get; init; }
    public string FileType { get; init; } = string.Empty;   // "pdf" | "docx"
    public string Source { get; init; } = string.Empty;     // "builder" | "upload"
    public long? FileSize { get; init; }
    public string? FileHash { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}

/// <summary>
/// Request DTO for creating a contract from an uploaded DOCX file.
/// Received via multipart/form-data from the controller.
/// </summary>
public record CreateContractFromUploadRequest
{
    public Guid CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public ContractTemplateType ContractType { get; init; } = ContractTemplateType.Other;
    public string? Description { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for uploading a new version of an existing contract (DOCX).
/// Received via multipart/form-data from the controller.
/// </summary>
public record UploadContractVersionRequest
{
    public string? Notes { get; init; }
}
