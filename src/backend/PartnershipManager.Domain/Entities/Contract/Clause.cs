// F3-ENT-001 cont.: Clause Entity
// File: src/backend/PartnershipManager.Domain/Entities/Contract/Clause.cs
// Author: GitHub Copilot  
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Standardized clause library for building contracts dynamically
/// </summary>
public class Clause : BaseEntity
{
    #region Properties

    /// <summary>
    /// Client this clause belongs to
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Human-readable clause name (e.g., "Confidentiality Obligations")
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of the clause
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Unique code identifier (e.g., "CONF-001")
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Clause content with {{variables}} support
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Category/type of this clause
    /// </summary>
    public ClauseType ClauseType { get; private set; }

    /// <summary>
    /// Whether this clause must be included in contracts
    /// </summary>
    public bool IsMandatory { get; private set; }

    /// <summary>
    /// Tags for categorization (serialized JSON)
    /// </summary>
    public List<string> Tags { get; private set; } = new();

    /// <summary>
    /// Display order in clause lists
    /// </summary>
    public int DisplayOrder { get; private set; } = 999;

    /// <summary>
    /// Clause version number
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Whether this clause can be used in new contracts
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new clause
    /// </summary>
    public static Clause Create(
        Guid clientId,
        string name,
        string code,
        string content,
        ClauseType clauseType,
        bool isMandatory = false,
        string? description = null,
        List<string>? tags = null,
        int displayOrder = 999,
        Guid? createdBy = null)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(code, nameof(code));
        ValidateRequired(content, nameof(content));

        return new Clause
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Name = name,
            Code = code,
            Description = description ?? string.Empty,
            Content = content,
            ClauseType = clauseType,
            IsMandatory = isMandatory,
            Tags = tags ?? new(),
            DisplayOrder = displayOrder,
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
    /// Update clause content and increment version
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
    /// Update clause metadata
    /// </summary>
    public void UpdateMetadata(
        string? name = null,
        string? description = null,
        List<string>? tags = null,
        bool? isMandatory = null,
        int? displayOrder = null,
        bool? isActive = null,
        Guid? updatedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (description != null)
            Description = description;

        if (tags != null)
            Tags = tags;

        if (isMandatory.HasValue)
            IsMandatory = isMandatory.Value;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Deactivate clause (archive without deleting)
    /// </summary>
    public void Deactivate(Guid? updatedBy = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Extract variable placeholders from clause content
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
