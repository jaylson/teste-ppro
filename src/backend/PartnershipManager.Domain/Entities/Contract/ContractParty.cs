// F3-ENT-001 cont.: ContractParty Entity
// File: src/backend/PartnershipManager.Domain/Entities/Contract/ContractParty.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Parties involved in a contract (signers, recipients, etc)
/// </summary>
public class ContractParty : BaseEntity
{
    #region Properties

    /// <summary>
    /// Contract this party is associated with
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// Role of this party (signer, recipient, witness, etc)
    /// </summary>
    public string PartyType { get; private set; } = "signer";

    /// <summary>
    /// Full name of the party
    /// </summary>
    public string PartyName { get; private set; } = string.Empty;

    /// <summary>
    /// Email address for notifications and signing
    /// </summary>
    public string PartyEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Optional: Internal user associated with this party
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Optional: Shareholder associated with this party
    /// </summary>
    public Guid? ShareholderId { get; private set; }

    /// <summary>
    /// Current signature status
    /// </summary>
    public SignatureStatus SignatureStatus { get; private set; } = SignatureStatus.Pending;

    /// <summary>
    /// When this party signed the contract
    /// </summary>
    public DateTime? SignatureDate { get; private set; }

    /// <summary>
    /// Token for signing service integration (e.g., ClickSign token)
    /// </summary>
    public string? SignatureToken { get; private set; }

    /// <summary>
    /// External ID from signing service
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Order for multi-signature workflows
    /// </summary>
    public int SequenceOrder { get; private set; } = 1;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new contract party
    /// </summary>
    public static ContractParty Create(
        Guid contractId,
        string partyName,
        string partyEmail,
        string partyType = "signer",
        Guid? userId = null,
        Guid? shareholderId = null,
        int sequenceOrder = 1)
    {
        ValidateRequired(partyName, nameof(partyName));
        ValidateRequired(partyEmail, nameof(partyEmail));
        ValidateEmail(partyEmail);

        return new ContractParty
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            PartyType = partyType,
            PartyName = partyName,
            PartyEmail = partyEmail,
            UserId = userId,
            ShareholderId = shareholderId,
            SignatureStatus = SignatureStatus.Pending,
            SequenceOrder = sequenceOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Set this party as waiting for signature
    /// </summary>
    public void SetWaitingSignature(string? signatureToken = null, string? externalId = null)
    {
        SignatureStatus = SignatureStatus.WaitingSignature;
        SignatureToken = signatureToken;
        ExternalId = externalId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark party signature as completed
    /// </summary>
    public void MarkAsSigned(string? externalId = null)
    {
        SignatureStatus = SignatureStatus.Signed;
        SignatureDate = DateTime.UtcNow;
        ExternalId = externalId ?? ExternalId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reject signature
    /// </summary>
    public void RejectSignature()
    {
        SignatureStatus = SignatureStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark signature as expired
    /// </summary>
    public void ExpireSignature()
    {
        SignatureStatus = SignatureStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update party information
    /// </summary>
    public void UpdateInfo(
        string? partyName = null,
        string? partyEmail = null,
        string? partyType = null)
    {
        if (!string.IsNullOrWhiteSpace(partyName))
            PartyName = partyName;

        if (!string.IsNullOrWhiteSpace(partyEmail))
        {
            ValidateEmail(partyEmail);
            PartyEmail = partyEmail;
        }

        if (!string.IsNullOrWhiteSpace(partyType))
            PartyType = partyType;

        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Validation

    private static void ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} cannot be empty", fieldName);
    }

    private static void ValidateEmail(string email)
    {
        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException("Invalid email format", nameof(email));
    }

    #endregion
}
