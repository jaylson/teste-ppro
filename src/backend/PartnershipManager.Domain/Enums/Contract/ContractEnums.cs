// F3-ENT-002: Contract-related enums
// File: src/backend/PartnershipManager.Domain/Enums/Contract/ContractEnums.cs
// Author: GitHub Copilot
// Date: 13/02/2026

namespace PartnershipManager.Domain.Enums;

/// <summary>
/// Types of contract templates
/// </summary>
public enum ContractTemplateType
{
    StockOption = 0,
    ShareholdersAgreement = 1,
    NDA = 2,
    Investment = 3,
    Employment = 4,
    ServiceAgreement = 5,
    Partnership = 6,
    Confidentiality = 7,
    Other = 99
}

/// <summary>
/// Categories/types of clauses for contractual content
/// </summary>
public enum ClauseType
{
    Governance = 0,
    RightsObligations = 1,
    Compliance = 2,
    Financial = 3,
    Termination = 4,
    Confidentiality = 5,
    DisputeResolution = 6,
    Amendments = 7,
    General = 99
}

/// <summary>
/// Contract lifecycle and workflow statuses
/// </summary>
public enum ContractStatus
{
    Draft = 0,
    PendingReview = 1,
    Approved = 2,
    SentForSignature = 3,
    PartiallySigned = 4,
    Signed = 5,
    Executed = 6,
    Expired = 7,
    Cancelled = 8
}

/// <summary>
/// Status of individual party signatures within a contract
/// </summary>
public enum SignatureStatus
{
    Pending = 0,
    WaitingSignature = 1,
    Signed = 2,
    Rejected = 3,
    Expired = 4
}

/// <summary>
/// Extension methods for enum descriptions
/// </summary>
public static class ContractEnumExtensions
{
    public static string ToDisplayName(this ContractTemplateType type) => type switch
    {
        ContractTemplateType.StockOption => "Stock Option Agreement",
        ContractTemplateType.ShareholdersAgreement => "Shareholders Agreement",
        ContractTemplateType.NDA => "Non-Disclosure Agreement",
        ContractTemplateType.Investment => "Investment Agreement",
        ContractTemplateType.Employment => "Employment Contract",
        ContractTemplateType.ServiceAgreement => "Service Agreement",
        ContractTemplateType.Partnership => "Partnership Agreement",
        ContractTemplateType.Confidentiality => "Confidentiality Agreement",
        _ => "Other"
    };

    public static string ToDisplayName(this ClauseType type) => type switch
    {
        ClauseType.Governance => "Governance",
        ClauseType.RightsObligations => "Rights & Obligations",
        ClauseType.Compliance => "Compliance",
        ClauseType.Financial => "Financial",
        ClauseType.Termination => "Termination",
        ClauseType.Confidentiality => "Confidentiality",
        ClauseType.DisputeResolution => "Dispute Resolution",
        ClauseType.Amendments => "Amendments",
        _ => "General"
    };

    public static string ToDisplayName(this ContractStatus status) => status switch
    {
        ContractStatus.Draft => "Draft",
        ContractStatus.PendingReview => "Pending Review",
        ContractStatus.Approved => "Approved",
        ContractStatus.SentForSignature => "Sent for Signature",
        ContractStatus.PartiallySigned => "Partially Signed",
        ContractStatus.Signed => "Signed",
        ContractStatus.Executed => "Executed",
        ContractStatus.Expired => "Expired",
        ContractStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    public static string ToDisplayName(this SignatureStatus status) => status switch
    {
        SignatureStatus.Pending => "Pending",
        SignatureStatus.WaitingSignature => "Waiting Signature",
        SignatureStatus.Signed => "Signed",
        SignatureStatus.Rejected => "Rejected",
        SignatureStatus.Expired => "Expired",
        _ => "Unknown"
    };
}
