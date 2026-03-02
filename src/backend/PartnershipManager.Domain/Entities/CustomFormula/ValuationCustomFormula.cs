namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Container for a company's custom valuation formula definition.
/// The actual expression and variables live in ValuationFormulaVersion (immutable snapshots).
/// Editing a formula creates a new version — old valuations keep their version reference.
/// </summary>
public class ValuationCustomFormula : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? SectorTag { get; private set; }

    /// <summary>Points to the latest version. Null until the first version is saved.</summary>
    public Guid? CurrentVersionId { get; private set; }

    public bool IsActive { get; private set; } = true;

    private ValuationCustomFormula() { }

    public static ValuationCustomFormula Create(
        Guid clientId,
        Guid companyId,
        string name,
        string? description = null,
        string? sectorTag = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da fórmula é obrigatório.", nameof(name));

        var f = new ValuationCustomFormula
        {
            ClientId = clientId,
            CompanyId = companyId,
            Name = name.Trim(),
            Description = description?.Trim(),
            SectorTag = sectorTag?.Trim().ToLowerInvariant(),
            IsActive = true
        };

        f.CreatedBy = createdBy;
        f.UpdatedBy = createdBy;
        return f;
    }

    public void UpdateMetadata(string name, string? description, string? sectorTag, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da fórmula é obrigatório.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        SectorTag = sectorTag?.Trim().ToLowerInvariant();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Called by service after persisting a new version.</summary>
    public void SetCurrentVersion(Guid versionId, Guid updatedBy)
    {
        CurrentVersionId = versionId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(Guid updatedBy)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
