namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Supporting document attached to a specific valuation.
/// Upload and verification handled by DocumentService / ContractStorageService pattern.
/// </summary>
public class ValuationDocument : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid ValuationId { get; private set; }

    // File metadata
    public string FileName { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string? DownloadUrl { get; private set; }

    // Verification
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedBy { get; private set; }

    private ValuationDocument() { }

    public static ValuationDocument Create(
        Guid clientId,
        Guid valuationId,
        string fileName,
        long fileSizeBytes,
        string mimeType,
        string storagePath,
        string? downloadUrl = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(fileName));
        if (fileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileSizeBytes), "Tamanho do arquivo deve ser positivo.");
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Path de armazenamento é obrigatório.", nameof(storagePath));

        var d = new ValuationDocument
        {
            ClientId = clientId,
            ValuationId = valuationId,
            FileName = fileName.Trim(),
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType.Trim(),
            StoragePath = storagePath.Trim(),
            DownloadUrl = downloadUrl?.Trim(),
            IsVerified = false
        };

        d.CreatedBy = createdBy;
        d.UpdatedBy = createdBy;
        return d;
    }

    public void Verify(Guid verifiedBy)
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        UpdatedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDownloadUrl(string downloadUrl, Guid updatedBy)
    {
        DownloadUrl = downloadUrl?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
