// F3-ENT-001 cont.: ContractClause Entity
// File: src/backend/PartnershipManager.Domain/Entities/Contract/ContractClause.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Junction entity: specific clause instance in a contract with customizations
/// </summary>
public class ContractClause : BaseEntity
{
    #region Properties

    /// <summary>
    /// Contract this clause belongs to
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// Reference to the base clause template
    /// </summary>
    public Guid ClauseId { get; private set; }

    /// <summary>
    /// Custom content for this clause in this contract (overrides base clause)
    /// </summary>
    public string? CustomContent { get; private set; }

    /// <summary>
    /// Order clauses appear in the contract
    /// </summary>
    public int DisplayOrder { get; private set; } = 999;

    /// <summary>
    /// Is this clause required in this contract
    /// </summary>
    public bool IsMandatory { get; private set; }

    /// <summary>
    /// Variable values specific to this clause instance
    /// </summary>
    public Dictionary<string, string> ClauseVariables { get; private set; } = new();

    /// <summary>
    /// Notes about this clause in this contract
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Navigation: Reference to base clause
    /// Can be used: from ClauseId to fetch full clause
    /// </summary>
    public Clause? BaseClause { get; set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a contract clause from a library clause
    /// </summary>
    public static ContractClause Create(
        Guid contractId,
        Guid clauseId,
        int displayOrder = 999,
        bool isMandatory = false,
        Dictionary<string, string>? variables = null,
        string? customContent = null,
        string? notes = null,
        Guid? createdBy = null)
    {
        ValidateNotEmpty(contractId, nameof(contractId));
        ValidateNotEmpty(clauseId, nameof(clauseId));

        return new ContractClause
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            ClauseId = clauseId,
            CustomContent = customContent,
            DisplayOrder = displayOrder,
            IsMandatory = isMandatory,
            ClauseVariables = variables ?? new(),
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update clause customization
    /// </summary>
    public void UpdateCustomization(
        string? customContent = null,
        int? displayOrder = null,
        Dictionary<string, string>? variables = null,
        string? notes = null,
        Guid? updatedBy = null)
    {
        if (customContent != null)
            CustomContent = customContent;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        if (variables != null)
            ClauseVariables = variables;

        if (notes != null)
            Notes = notes;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Set variable value for this clause
    /// </summary>
    public void SetVariable(string variableName, string variableValue)
    {
        ValidateRequired(variableName, nameof(variableName));
        
        ClauseVariables[variableName] = variableValue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set multiple variables at once
    /// </summary>
    public void SetVariables(Dictionary<string, string> variables)
    {
        if (variables == null)
            return;

        foreach (var kvp in variables)
        {
            ClauseVariables[kvp.Key] = kvp.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get the effective content (custom or base clause content)
    /// Note: This requires the BaseClause to be loaded
    /// </summary>
    public string GetEffectiveContent()
    {
        if (!string.IsNullOrEmpty(CustomContent))
            return CustomContent;

        if (BaseClause != null)
            return BaseClause.Content;

        return string.Empty;
    }

    /// <summary>
    /// Substitute variables in clause content
    /// </summary>
    public string GetRenderedContent()
    {
        var content = GetEffectiveContent();

        foreach (var kvp in ClauseVariables)
        {
            content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return content;
    }

    /// <summary>
    /// Update display order
    /// </summary>
    public void UpdateOrder(int newOrder, Guid? updatedBy = null)
    {
        DisplayOrder = newOrder;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
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
