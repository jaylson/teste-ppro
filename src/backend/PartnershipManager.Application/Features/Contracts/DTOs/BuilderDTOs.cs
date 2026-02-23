// F3-BLD-BE-002: Contract Builder DTOs
// File: src/backend/PartnershipManager.Application/Features/Contracts/DTOs/BuilderDTOs.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Contracts.DTOs;

#region Step 1:  Select Template

/// <summary>
/// Request to start a new contract builder session - Step 1
/// </summary>
public record StartBuilderRequest
{
    public Guid CompanyId { get; init; }
    public Guid? TemplateId { get; init; }
    public string? Title { get; init; }
}

/// <summary>
/// Response with builder session initialized
/// </summary>
public record BuilderSessionResponse
{
    public Guid SessionId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid? TemplateId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public ContractTemplateType? TemplateType { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CurrentStep { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

#endregion

#region Step 2: Add Parties

/// <summary>
/// Request to add parties (signers) to contract - Step 2
/// </summary>
public record AddPartiesRequest
{
    public Guid SessionId { get; init; }
    public List<PartyInfo> Parties { get; init; } = new();
}

/// <summary>
/// Information about a contract party/signer
/// </summary>
public record PartyInfo
{
    public string PartyType { get; init; } = string.Empty; // "Signer", "Witness", "Recipient"
    public string PartyName { get; init; } = string.Empty;
    public string PartyEmail { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public Guid? ShareholderId { get; init; }
    public int SequenceOrder { get; init; }
}

/// <summary>
/// Response with parties added
/// </summary>
public record PartiesResponse
{
    public Guid SessionId { get; init; }
    public List<PartyInfo> Parties { get; init; } = new();
    public int CurrentStep { get; init; }
}

#endregion

#region Step 3: Select Clauses

/// <summary>
/// Request to select clauses for contract - Step 3
/// </summary>
public record SelectClausesRequest
{
    public Guid SessionId { get; init; }
    public List<ClauseSelection> Clauses { get; init; } = new();
}

/// <summary>
/// Information about a selected clause
/// </summary>
public record ClauseSelection
{
    public Guid ClauseId { get; init; }
    public int DisplayOrder { get; init; }
    public string? CustomContent { get; init; }
    public bool IsMandatory { get; init; }
    public Dictionary<string, string> Variables { get; init; } = new();
}

/// <summary>
/// Response with clauses selected
/// </summary>
public record ClausesResponse
{
    public Guid SessionId { get; init; }
    public List<ClauseSelection> Clauses { get; init; } = new();
    public int CurrentStep { get; init; }
}

#endregion

#region Step 4: Fill Data (Variables)

/// <summary>
/// Request to fill contract data/variables - Step 4
/// </summary>
public record FillDataRequest
{
    public Guid SessionId { get; init; }
    public Dictionary<string, string> Variables { get; init; } = new();
    public DateTime? ContractDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Response with data filled
/// </summary>
public record DataResponse
{
    public Guid SessionId { get; init; }
    public Dictionary<string, string> Variables { get; init; } = new();
    public DateTime? ContractDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
    public string? Notes { get; init; }
    public List<string> RequiredVariables { get; init; } = new();
    public List<string> MissingVariables { get; init; } = new();
    public int CurrentStep { get; init; }
}

#endregion

#region Step 5: Preview & Generate

/// <summary>
/// Request to preview contract before generation - Step 5
/// </summary>
public record PreviewContractRequest
{
    public Guid SessionId { get; init; }
}

/// <summary>
/// Response with contract preview
/// </summary>
public record PreviewContractResponse
{
    public Guid SessionId { get; init; }
    public string HtmlPreview { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public List<string> ValidationErrors { get; init; } = new();
    public int CurrentStep { get; init; }
}

/// <summary>
/// Request to generate final contract - Step 5
/// </summary>
public record GenerateContractRequest
{
    public Guid SessionId { get; init; }
    public bool SendForSignature { get; init; } = false;
}

/// <summary>
/// Response with generated contract
/// </summary>
public record GenerateContractResponse
{
    public Guid ContractId { get; init; }
    public Guid SessionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public ContractStatus Status { get; init; }
    public string? DocumentPath { get; init; }
    public long? DocumentSize { get; init; }
    public bool SentForSignature { get; init; }
    public DateTime GeneratedAt { get; init; }
}

#endregion

#region Session Management

/// <summary>
/// Request to get current builder session state
/// </summary>
public record GetSessionRequest
{
    public Guid SessionId { get; init; }
}

/// <summary>
/// Complete builder session state
/// </summary>
public record CompleteSessionResponse
{
    public Guid SessionId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid? TemplateId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CurrentStep { get; init; }
    public List<PartyInfo> Parties { get; init; } = new();
    public List<ClauseSelection> Clauses { get; init; } = new();
    public Dictionary<string, string> Variables { get; init; } = new();
    public DateTime? ContractDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Description { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Request to delete/cancel builder session
/// </summary>
public record CancelSessionRequest
{
    public Guid SessionId { get; init; }
}

#endregion

#region Validation

/// <summary>
/// Validation result for builder step
/// </summary>
public record StepValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}

#endregion
