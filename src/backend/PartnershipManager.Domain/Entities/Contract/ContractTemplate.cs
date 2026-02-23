// F3-ENT-001: Contract Template Entity
// File: src/backend/PartnershipManager.Domain/Entities/Contract/ContractTemplate.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Reusable contract template with dynamic fields and customization support
/// </summary>
public class ContractTemplate : BaseEntity
{
    #region Properties

    /// <summary>
    /// Client this template belongs to
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Optional: Company-specific template
    /// </summary>
    public Guid? CompanyId { get; private set; }

    /// <summary>
    /// Template name/title
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of what this template is for
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Unique code identifier (e.g., "SA-001")
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Type of contract from the template
    /// </summary>
    public ContractTemplateType TemplateType { get; private set; }

    /// <summary>
    /// Template content with {{variables}} support
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Default status when contract is created from this template
    /// </summary>
    public ContractStatus DefaultStatus { get; private set; } = ContractStatus.Draft;

    /// <summary>
    /// Tags for categorization (serialized JSON)
    /// </summary>
    public List<string> Tags { get; private set; } = new();

    /// <summary>
    /// Template version number
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Whether this template can be used for new contracts
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new contract template
    /// </summary>
    public static ContractTemplate Create(
        Guid clientId,
        string name,
        string code,
        ContractTemplateType templateType,
        string content,
        string? description = null,
        Guid? companyId = null,
        ContractStatus? defaultStatus = null,
        List<string>? tags = null,
        Guid? createdBy = null)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(code, nameof(code));
        ValidateRequired(content, nameof(content));

        return new ContractTemplate
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CompanyId = companyId,
            Name = name,
            Code = code,
            Description = description ?? string.Empty,
            TemplateType = templateType,
            Content = content,
            DefaultStatus = defaultStatus ?? ContractStatus.Draft,
            Tags = tags ?? new(),
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update template content and increment version
    /// </summary>
    public void UpdateContent(string newContent, Guid? updatedBy = null)
    {
        ValidateRequired(newContent, nameof(newContent));
        
        Content = newContent;
        Version++;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Update template metadata
    /// </summary>
    public void UpdateMetadata(
        string? name = null,
        string? description = null,
        List<string>? tags = null,
        bool? isActive = null,
        Guid? updatedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (description != null)
            Description = description;

        if (tags != null)
            Tags = tags;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Deactivate template (archive without deleting)
    /// </summary>
    public void Deactivate(Guid? updatedBy = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Clone this template creating a new independent copy
    /// </summary>
    public ContractTemplate Clone(string? newCode = null, Guid? createdBy = null)
    {
        return new ContractTemplate
        {
            Id = Guid.NewGuid(),
            ClientId = ClientId,
            CompanyId = CompanyId,
            Name = $"{Name} (Clone)",
            Code = newCode ?? $"{Code}-CLONE-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Description = Description,
            TemplateType = TemplateType,
            Content = Content,
            DefaultStatus = DefaultStatus,
            Tags = new List<string>(Tags),
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Extract variable placeholders from template content
    /// </summary>
    public List<string> ExtractVariables()
    {
        var variables = new List<string>();
        var regex = new System.Text.RegularExpressions.Regex(@"\{\{(\w+)\}\}");
        
        foreach (System.Text.RegularExpressions.Match match in regex.Matches(Content))
        {
            var variable = match.Groups[1].Value;
            if (!variables.Contains(variable))
                variables.Add(variable);
        }

        return variables;
    }

    #endregion

    #region Validation

    private static void ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} cannot be empty", fieldName);
    }

    #endregion
}
