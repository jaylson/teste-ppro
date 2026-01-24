using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Represents an immutable record of a share transaction.
/// This is an append-only ledger - transactions cannot be modified or deleted.
/// </summary>
public class ShareTransaction
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string? TransactionNumber { get; private set; }
    public DateTime ReferenceDate { get; private set; }
    
    // Share and Class info
    public Guid? ShareId { get; private set; }
    public Guid ShareClassId { get; private set; }
    
    // Quantity and Value
    public decimal Quantity { get; private set; }
    public decimal PricePerShare { get; private set; }
    public decimal TotalValue => Quantity * PricePerShare;
    
    // Parties involved
    public Guid? FromShareholderId { get; private set; }
    public Guid? ToShareholderId { get; private set; }
    
    // Additional info
    public string? Reason { get; private set; }
    public string? DocumentReference { get; private set; }
    public string? Notes { get; private set; }
    
    // Approval
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    
    // Audit (immutable - no UpdatedAt)
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    // Navigation properties (populated by repository)
    public string? FromShareholderName { get; private set; }
    public string? ToShareholderName { get; private set; }
    public string? ShareClassName { get; private set; }
    public string? ShareClassCode { get; private set; }
    public string? CompanyName { get; private set; }
    public string? ApprovedByName { get; private set; }

    private ShareTransaction() 
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new share issuance transaction.
    /// </summary>
    public static ShareTransaction CreateIssuance(
        Guid clientId,
        Guid companyId,
        Guid shareClassId,
        Guid toShareholderId,
        decimal quantity,
        decimal pricePerShare,
        DateTime referenceDate,
        string? transactionNumber = null,
        string? reason = null,
        string? documentReference = null,
        string? notes = null,
        Guid? approvedBy = null,
        Guid? createdBy = null)
    {
        ValidateQuantity(quantity);
        ValidatePrice(pricePerShare);

        return new ShareTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            TransactionType = TransactionType.Issue,
            TransactionNumber = transactionNumber,
            ReferenceDate = referenceDate,
            ShareClassId = shareClassId,
            ToShareholderId = toShareholderId,
            Quantity = quantity,
            PricePerShare = pricePerShare,
            Reason = reason,
            DocumentReference = documentReference,
            Notes = notes,
            ApprovedBy = approvedBy,
            ApprovedAt = approvedBy.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a share transfer transaction.
    /// </summary>
    public static ShareTransaction CreateTransfer(
        Guid clientId,
        Guid companyId,
        Guid shareClassId,
        Guid fromShareholderId,
        Guid toShareholderId,
        decimal quantity,
        decimal pricePerShare,
        DateTime referenceDate,
        Guid? shareId = null,
        string? transactionNumber = null,
        string? reason = null,
        string? documentReference = null,
        string? notes = null,
        Guid? approvedBy = null,
        Guid? createdBy = null)
    {
        ValidateQuantity(quantity);
        ValidatePrice(pricePerShare);

        if (fromShareholderId == toShareholderId)
            throw new ArgumentException("Cannot transfer shares to the same shareholder");

        return new ShareTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            TransactionType = TransactionType.Transfer,
            TransactionNumber = transactionNumber,
            ReferenceDate = referenceDate,
            ShareId = shareId,
            ShareClassId = shareClassId,
            FromShareholderId = fromShareholderId,
            ToShareholderId = toShareholderId,
            Quantity = quantity,
            PricePerShare = pricePerShare,
            Reason = reason,
            DocumentReference = documentReference,
            Notes = notes,
            ApprovedBy = approvedBy,
            ApprovedAt = approvedBy.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a share cancellation transaction.
    /// </summary>
    public static ShareTransaction CreateCancellation(
        Guid clientId,
        Guid companyId,
        Guid shareClassId,
        Guid fromShareholderId,
        decimal quantity,
        decimal pricePerShare,
        DateTime referenceDate,
        Guid? shareId = null,
        string? transactionNumber = null,
        string reason = "Cancellation",
        string? documentReference = null,
        string? notes = null,
        Guid? approvedBy = null,
        Guid? createdBy = null)
    {
        ValidateQuantity(quantity);

        return new ShareTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            TransactionType = TransactionType.Cancel,
            TransactionNumber = transactionNumber,
            ReferenceDate = referenceDate,
            ShareId = shareId,
            ShareClassId = shareClassId,
            FromShareholderId = fromShareholderId,
            Quantity = quantity,
            PricePerShare = pricePerShare,
            Reason = reason,
            DocumentReference = documentReference,
            Notes = notes,
            ApprovedBy = approvedBy,
            ApprovedAt = approvedBy.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a share conversion transaction.
    /// </summary>
    public static ShareTransaction CreateConversion(
        Guid clientId,
        Guid companyId,
        Guid fromShareClassId,
        Guid fromShareholderId,
        decimal quantity,
        decimal pricePerShare,
        DateTime referenceDate,
        Guid? shareId = null,
        string? transactionNumber = null,
        string? reason = null,
        string? documentReference = null,
        string? notes = null,
        Guid? approvedBy = null,
        Guid? createdBy = null)
    {
        ValidateQuantity(quantity);

        return new ShareTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            TransactionType = TransactionType.Convert,
            TransactionNumber = transactionNumber,
            ReferenceDate = referenceDate,
            ShareId = shareId,
            ShareClassId = fromShareClassId,
            FromShareholderId = fromShareholderId,
            ToShareholderId = fromShareholderId, // Same person receives converted shares
            Quantity = quantity,
            PricePerShare = pricePerShare,
            Reason = reason ?? "Class conversion",
            DocumentReference = documentReference,
            Notes = notes,
            ApprovedBy = approvedBy,
            ApprovedAt = approvedBy.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Links this transaction to a specific share record.
    /// </summary>
    public void SetShareId(Guid shareId)
    {
        ShareId = shareId;
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
    }
}
