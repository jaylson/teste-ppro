// F3-ENT-002: Contract-related enums
// File: src/backend/PartnershipManager.Domain/Enums/Contract/ContractEnums.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using System.Runtime.Serialization;

namespace PartnershipManager.Domain.Enums;

/// <summary>
/// Types of contract templates
/// </summary>
public enum ContractTemplateType
{
    [EnumMember(Value = "stock_option")]
    StockOption = 0,
    
    [EnumMember(Value = "shareholders_agreement")]
    ShareholdersAgreement = 1,
    
    [EnumMember(Value = "nda")]
    NDA = 2,
    
    [EnumMember(Value = "investment")]
    Investment = 3,
    
    [EnumMember(Value = "employment")]
    Employment = 4,
    
    [EnumMember(Value = "service_agreement")]
    ServiceAgreement = 5,
    
    [EnumMember(Value = "partnership")]
    Partnership = 6,
    
    [EnumMember(Value = "confidentiality")]
    Confidentiality = 7,
    
    [EnumMember(Value = "other")]
    Other = 99
}

/// <summary>
/// Categories/types of clauses for contractual content
/// </summary>
public enum ClauseType
{
    [EnumMember(Value = "governance")]
    Governance = 0,
    
    [EnumMember(Value = "rights_obligations")]
    RightsObligations = 1,
    
    [EnumMember(Value = "compliance")]
    Compliance = 2,
    
    [EnumMember(Value = "financial")]
    Financial = 3,
    
    [EnumMember(Value = "termination")]
    Termination = 4,
    
    [EnumMember(Value = "confidentiality")]
    Confidentiality = 5,
    
    [EnumMember(Value = "dispute_resolution")]
    DisputeResolution = 6,
    
    [EnumMember(Value = "amendments")]
    Amendments = 7,
    
    [EnumMember(Value = "general")]
    General = 99
}

/// <summary>
/// Contract lifecycle and workflow statuses
/// </summary>
public enum ContractStatus
{
    [EnumMember(Value = "draft")]
    Draft = 0,
    
    [EnumMember(Value = "pending_review")]
    PendingReview = 1,
    
    [EnumMember(Value = "approved")]
    Approved = 2,
    
    [EnumMember(Value = "sent_for_signature")]
    SentForSignature = 3,
    
    [EnumMember(Value = "partially_signed")]
    PartiallySigned = 4,
    
    [EnumMember(Value = "signed")]
    Signed = 5,
    
    [EnumMember(Value = "executed")]
    Executed = 6,
    
    [EnumMember(Value = "expired")]
    Expired = 7,
    
    [EnumMember(Value = "cancelled")]
    Cancelled = 8
}

/// <summary>
/// Status of individual party signatures within a contract
/// </summary>
public enum SignatureStatus
{
    [EnumMember(Value = "pending")]
    Pending = 0,
    
    [EnumMember(Value = "waiting_signature")]
    WaitingSignature = 1,
    
    [EnumMember(Value = "signed")]
    Signed = 2,
    
    [EnumMember(Value = "rejected")]
    Rejected = 3,
    
    [EnumMember(Value = "expired")]
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
