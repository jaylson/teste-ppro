// F3-BLD-BE-002: Contract Builder Session Model
// File: src/backend/PartnershipManager.Application/Features/Contracts/Models/BuilderSession.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Application.Features.Contracts.DTOs;

namespace PartnershipManager.Application.Features.Contracts.Models;

/// <summary>
/// In-memory model for contract builder session state
/// Stores progress as user goes through the 5-step wizard
/// </summary>
public class BuilderSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CurrentStep { get; set; } = 1;
    
    // Step 2: Parties
    public List<PartyInfo> Parties { get; set; } = new();
    
    // Step 3: Clauses
    public List<ClauseSelection> Clauses { get; set; } = new();
    
    // Step 4: Variables and Data
    public Dictionary<string, string> Variables { get; set; } = new();
    public DateTime? ContractDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    
    // Step 5: Preview cache
    public string? HtmlPreview { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // Session expiration
    public Guid? CreatedBy { get; set; }
    
    // Generated contract ID (after completion)
    public Guid? GeneratedContractId { get; set; }

    /// <summary>
    /// Check if session is expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Move to next step
    /// </summary>
    public void AdvanceStep()
    {
        if (CurrentStep < 5)
        {
            CurrentStep++;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Move to previous step
    /// </summary>
    public void GoToPreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Move to specific step
    /// </summary>
    public void SetStep(int step)
    {
        if (step >= 1 && step <= 5)
        {
            CurrentStep = step;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Touch session to extend expiration
    /// </summary>
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
        // Extend expiration by 30 minutes from now
        ExpiresAt = DateTime.UtcNow.AddMinutes(30);
    }

    /// <summary>
    /// Convert to CompleteSessionResponse DTO
    /// </summary>
    public CompleteSessionResponse ToResponse()
    {
        return new CompleteSessionResponse
        {
            SessionId = SessionId,
            CompanyId = CompanyId,
            TemplateId = TemplateId,
            Title = Title,
            CurrentStep = CurrentStep,
            Parties = Parties,
            Clauses = Clauses,
            Variables = Variables,
            ContractDate = ContractDate,
            ExpirationDate = ExpirationDate,
            Description = Description,
            Notes = Notes,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
