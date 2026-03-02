namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Central polymorphic document repository.
/// Can be associated with any entity (valuation, financial_period, contract) via EntityType + EntityId,
/// or used as a standalone company document.
/// </summary>
public class Document : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }

    // Document metadata
    public string Name { get; private set; } = string.Empty;
    public string DocumentType { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // File info
    public string FileName { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string? DownloadUrl { get; private set; }

    // Polymorphic entity association (both null = standalone)
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }

    // Visibility
    public string Visibility { get; private set; } = DocumentVisibility.Admin;

    // Verification
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedBy { get; private set; }

    // Derived
    public bool IsLinkedToEntity => EntityType != null && EntityId.HasValue;
    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };

    private Document() { }

    public static Document Create(
        Guid clientId,
        Guid companyId,
        string name,
        string documentType,
        string fileName,
        long fileSizeBytes,
        string mimeType,
        string storagePath,
        string visibility = DocumentVisibility.Admin,
        string? description = null,
        string? entityType = null,
        Guid? entityId = null,
        string? downloadUrl = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do documento é obrigatório.", nameof(name));
        if (!DocumentTypes.All.Contains(documentType))
            throw new ArgumentException($"Tipo de documento inválido: {documentType}.", nameof(documentType));
        if (!DocumentVisibility.All.Contains(visibility))
            throw new ArgumentException($"Visibilidade inválida: {visibility}.", nameof(visibility));
        if (fileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileSizeBytes), "Tamanho do arquivo deve ser positivo.");
        if ((entityType == null) != (entityId == null))
            throw new ArgumentException("EntityType e EntityId devem ser ambos informados ou ambos nulos.");

        var d = new Document
        {
            ClientId = clientId,
            CompanyId = companyId,
            Name = name.Trim(),
            DocumentType = documentType,
            Description = description?.Trim(),
            FileName = fileName.Trim(),
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType.Trim(),
            StoragePath = storagePath.Trim(),
            DownloadUrl = downloadUrl?.Trim(),
            EntityType = entityType,
            EntityId = entityId,
            Visibility = visibility,
            IsVerified = false
        };

        d.CreatedBy = createdBy;
        d.UpdatedBy = createdBy;
        return d;
    }

    public void UpdateMetadata(string name, string? description, string visibility, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório.", nameof(name));
        if (!DocumentVisibility.All.Contains(visibility))
            throw new ArgumentException($"Visibilidade inválida: {visibility}.");

        Name = name.Trim();
        Description = description?.Trim();
        Visibility = visibility;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
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

public static class DocumentTypes
{
    public const string BalanceSheet = "balance_sheet";
    public const string IncomeStatement = "income_statement";
    public const string CashFlow = "cash_flow";
    public const string AuditReport = "audit_report";
    public const string Contract = "contract";
    public const string Certificate = "certificate";
    public const string Bylaws = "bylaws";
    public const string Minutes = "minutes";
    public const string Presentation = "presentation";
    public const string ValuationSupport = "valuation_support";
    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        BalanceSheet, IncomeStatement, CashFlow, AuditReport,
        Contract, Certificate, Bylaws, Minutes, Presentation,
        ValuationSupport, Other
    };
}

public static class DocumentVisibility
{
    public const string Admin = "admin";
    public const string Board = "board";
    public const string Shareholders = "shareholders";
    public const string Investors = "investors";
    public const string Public = "public";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Admin, Board, Shareholders, Investors, Public
    };
}

public static class DocumentEntityTypes
{
    public const string Valuation = "valuation";
    public const string FinancialPeriod = "financial_period";
    public const string Contract = "contract";
}
