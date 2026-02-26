using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Immutable ledger record of an exercise event. Never updated or soft-deleted.
/// </summary>
public class VestingTransaction
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid VestingGrantId { get; private set; }
    public Guid ShareholderId { get; private set; }
    public Guid CompanyId { get; private set; }

    public DateTime TransactionDate { get; private set; }
    public decimal SharesExercised { get; private set; }
    public decimal SharePriceAtExercise { get; private set; }
    public decimal StrikePrice { get; private set; }
    public decimal TotalExerciseValue => SharesExercised * SharePriceAtExercise;
    public decimal GainAmount => SharesExercised * (SharePriceAtExercise - StrikePrice);

    public Guid? ShareTransactionId { get; private set; }
    public VestingTransactionType TransactionType { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private VestingTransaction() { }

    public static VestingTransaction Create(
        Guid clientId,
        Guid vestingGrantId,
        Guid shareholderId,
        Guid companyId,
        DateTime transactionDate,
        decimal sharesExercised,
        decimal sharePriceAtExercise,
        decimal strikePrice,
        Guid createdBy,
        VestingTransactionType transactionType = VestingTransactionType.Exercise,
        Guid? shareTransactionId = null,
        string? notes = null)
    {
        if (sharesExercised <= 0)
            throw new ArgumentOutOfRangeException(nameof(sharesExercised), "Quantidade de ações deve ser positiva.");
        if (sharePriceAtExercise <= 0)
            throw new ArgumentOutOfRangeException(nameof(sharePriceAtExercise), "Preço de mercado deve ser positivo.");
        if (strikePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(strikePrice), "Preço de exercício não pode ser negativo.");

        return new VestingTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            ShareholderId = shareholderId,
            CompanyId = companyId,
            TransactionDate = transactionDate.Date,
            SharesExercised = sharesExercised,
            SharePriceAtExercise = sharePriceAtExercise,
            StrikePrice = strikePrice,
            TransactionType = transactionType,
            ShareTransactionId = shareTransactionId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Links this exercise transaction to the resulting share_transaction in the cap table.
    /// Can only be set once.
    /// </summary>
    public void LinkToShareTransaction(Guid shareTransactionId)
    {
        if (ShareTransactionId.HasValue)
            throw new InvalidOperationException("Transação já vinculada a um registro do cap table.");
        ShareTransactionId = shareTransactionId;
    }

    /// <summary>
    /// Reconstitutes a VestingTransaction from persistence.
    /// </summary>
    public static VestingTransaction Reconstitute(
        Guid id,
        Guid clientId,
        Guid vestingGrantId,
        Guid shareholderId,
        Guid companyId,
        DateTime transactionDate,
        decimal sharesExercised,
        decimal sharePriceAtExercise,
        decimal strikePrice,
        Guid? shareTransactionId,
        VestingTransactionType transactionType,
        string? notes,
        DateTime createdAt,
        Guid createdBy)
    {
        return new VestingTransaction
        {
            Id = id,
            ClientId = clientId,
            VestingGrantId = vestingGrantId,
            ShareholderId = shareholderId,
            CompanyId = companyId,
            TransactionDate = transactionDate,
            SharesExercised = sharesExercised,
            SharePriceAtExercise = sharePriceAtExercise,
            StrikePrice = strikePrice,
            ShareTransactionId = shareTransactionId,
            TransactionType = transactionType,
            Notes = notes,
            CreatedAt = createdAt,
            CreatedBy = createdBy
        };
    }
}
