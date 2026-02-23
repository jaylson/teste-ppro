// F3-ENT-001 cont.: Contract Entity
// File: src/backend/PartnershipManager.Domain/Entities/Contract/Contract.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Main contract entity representing a generated contract document
/// </summary>
public class Contract : BaseEntity
{
    #region Properties

    /// <summary>
    /// Client this contract belongs to
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Company this contract is for
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// Contract title/name
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Contract description/purpose
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Type of contract
    /// </summary>
    public ContractTemplateType ContractType { get; private set; }

    /// <summary>
    /// Optional: Reference to source template
    /// </summary>
    public Guid? TemplateId { get; private set; }

    /// <summary>
    /// Path to stored contract document (S3/Local)
    /// </summary>
    public string? DocumentPath { get; private set; }

    /// <summary>
    /// Document file size in bytes
    /// </summary>
    public long? DocumentSize { get; private set; }

    /// <summary>
    /// SHA-256 hash for integrity verification
    /// </summary>
    public string? DocumentHash { get; private set; }

    /// <summary>
    /// Current status in contract lifecycle
    /// </summary>
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;

    /// <summary>
    /// When contract becomes effective
    /// </summary>
    public DateTime? ContractDate { get; private set; }

    /// <summary>
    /// When contract expires (if applicable)
    /// </summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// Reference in external systems (e.g., ClickSign ID)
    /// </summary>
    public string? ExternalReference { get; private set; }

    /// <summary>
    /// Internal notes about the contract
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Navigation: Contract parties (signers, recipients)
    /// </summary>
    public List<ContractParty> Parties { get; private set; } = new();

    /// <summary>
    /// Navigation: Clauses in this contract
    /// </summary>
    public List<ContractClause> Clauses { get; private set; } = new();

    /// <summary>
    /// Current (latest) version number. 0 = no document yet.
    /// </summary>
    public int CurrentVersionNumber { get; private set; } = 0;

    /// <summary>
    /// Navigation: Full document version history
    /// </summary>
    public List<ContractVersion> Versions { get; private set; } = new();

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new contract from a template
    /// </summary>
    public static Contract Create(
        Guid clientId,
        Guid companyId,
        string title,
        ContractTemplateType contractType,
        Guid? templateId = null,
        string? description = null,
        DateTime? contractDate = null,
        DateTime? expirationDate = null,
        Guid? createdBy = null)
    {
        ValidateRequired(title, nameof(title));
        ValidateNotEmpty(clientId, nameof(clientId));
        ValidateNotEmpty(companyId, nameof(companyId));

        return new Contract
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            Title = title,
            Description = description ?? string.Empty,
            ContractType = contractType,
            TemplateId = templateId,
            Status = ContractStatus.Draft,
            ContractDate = contractDate,
            ExpirationDate = expirationDate,
            Parties = new(),
            Clauses = new(),
            Versions = new(),
            CurrentVersionNumber = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update contract metadata
    /// </summary>
    public void UpdateMetadata(
        string? title = null,
        string? description = null,
        DateTime? contractDate = null,
        DateTime? expirationDate = null,
        string? notes = null,
        Guid? updatedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (description != null)
            Description = description;

        if (contractDate.HasValue)
            ContractDate = contractDate;

        if (expirationDate.HasValue)
            ExpirationDate = expirationDate;

        if (notes != null)
            Notes = notes;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Set document after generation/upload
    /// </summary>
    public void SetDocument(string documentPath, long documentSize, string documentHash, Guid? updatedBy = null)
    {
        ValidateRequired(documentPath, nameof(documentPath));
        ValidateRequired(documentHash, nameof(documentHash));

        DocumentPath = documentPath;
        DocumentSize = documentSize;
        DocumentHash = documentHash;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Increments the version counter and returns the new version number.
    /// Must be called before persisting a new ContractVersion record.
    /// </summary>
    public int IncrementVersion(Guid? updatedBy = null)
    {
        CurrentVersionNumber++;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        return CurrentVersionNumber;
    }

    /// <summary>
    /// Submit contract for review
    /// </summary>
    public void SubmitForReview(Guid? updatedBy = null)
    {
        if (Status != ContractStatus.Draft)
            throw new InvalidOperationException($"Cannot submit for review from status {Status}");

        Status = ContractStatus.PendingReview;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Approve contract after review
    /// </summary>
    public void Approve(Guid? updatedBy = null)
    {
        if (Status != ContractStatus.PendingReview)
            throw new InvalidOperationException($"Cannot approve from status {Status}");

        Status = ContractStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Send contract for signature
    /// </summary>
    public void SendForSignature(Guid? updatedBy = null)
    {
        if (Status != ContractStatus.Approved && Status != ContractStatus.Draft)
            throw new InvalidOperationException($"Cannot send for signature from status {Status}");

        Status = ContractStatus.SentForSignature;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Mark as fully signed and executed
    /// </summary>
    public void MarkAsExecuted(Guid? updatedBy = null)
    {
        if (Status != ContractStatus.PartiallySigned && Status != ContractStatus.Signed)
            throw new InvalidOperationException($"Cannot execute from status {Status}");

        Status = ContractStatus.Executed;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Update signature tracking when parties sign
    /// </summary>
    public void UpdateSignatureProgress()
    {
        if (Parties.Count == 0)
            return;

        var allSigned = Parties.All(p => p.SignatureStatus == SignatureStatus.Signed);
        var anySigned = Parties.Any(p => p.SignatureStatus == SignatureStatus.Signed);

        if (allSigned)
            Status = ContractStatus.Signed;
        else if (anySigned && Status != ContractStatus.PartiallySigned)
            Status = ContractStatus.PartiallySigned;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancel contract
    /// </summary>
    public void Cancel(string? reason = null, Guid? updatedBy = null)
    {
        Status = ContractStatus.Cancelled;
        Notes = reason ?? Notes;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Add a party (signer) to the contract
    /// </summary>
    public void AddParty(ContractParty party)
    {
        if (party == null)
            throw new ArgumentNullException(nameof(party));

        if (Parties.Any(p => p.PartyEmail == party.PartyEmail && !p.IsDeleted))
            throw new InvalidOperationException($"Party with email {party.PartyEmail} already exists");

        Parties.Add(party);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a party from the contract
    /// </summary>
    public void RemoveParty(Guid partyId)
    {
        var party = Parties.FirstOrDefault(p => p.Id == partyId);
        if (party != null)
        {
            // Mark as deleted via BaseEntity property
            party.IsDeleted = true;
            party.UpdatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Add a clause to the contract
    /// </summary>
    public void AddClause(ContractClause clause)
    {
        if (clause == null)
            throw new ArgumentNullException(nameof(clause));

        Clauses.Add(clause);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a clause from the contract
    /// </summary>
    public void RemoveClause(Guid clauseId, Guid? updatedBy = null)
    {
        var clause = Clauses.FirstOrDefault(c => c.Id == clauseId);
        if (clause != null)
        {
            // Mark as deleted via BaseEntity property
            clause.IsDeleted = true;
            clause.UpdatedBy = updatedBy;
            clause.UpdatedAt = DateTime.UtcNow;
            
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }

    #endregion

    #region Validation

    private static void ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} cannot be empty", fieldName);
    }

    private static void ValidateNotEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{fieldName} cannot be empty", fieldName);
    }

    #endregion
}
