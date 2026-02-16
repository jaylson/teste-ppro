// F3-BLD-BE-001: Contract Generation Service Interface
// File: src/backend/PartnershipManager.Domain/Interfaces/Services/IContractGenerationService.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Domain.Interfaces.Services;

/// <summary>
/// Service for generating contracts from templates with variable substitution
/// </summary>
public interface IContractGenerationService
{
    /// <summary>
    /// Generate contract document from template with provided data
    /// </summary>
    /// <param name="contract">The contract entity</param>
    /// <param name="template">The template to use</param>
    /// <param name="clauses">List of clauses to include</param>
    /// <param name="variables">Dictionary of variable values for substitution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated contract content (HTML)</returns>
    Task<string> GenerateContractContentAsync(
        Contract contract,
        ContractTemplate template,
        List<Clause> clauses,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate PDF document from contract HTML content
    /// </summary>
    /// <param name="contract">The contract entity</param>
    /// <param name="htmlContent">HTML content to convert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateContractPdfAsync(
        Contract contract,
        string htmlContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate complete contract (content + PDF) from template
    /// </summary>
    /// <param name="contractId">Contract ID</param>
    /// <param name="templateId">Template ID</param>
    /// <param name="clauseIds">List of clause IDs to include</param>
    /// <param name="variables">Dictionary of variable values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple with HTML content and PDF bytes</returns>
    Task<(string HtmlContent, byte[] PdfBytes)> GenerateCompleteContractAsync(
        Guid contractId,
        Guid templateId,
        List<Guid> clauseIds,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract variables from template content
    /// </summary>
    /// <param name="content">Template or clause content</param>
    /// <returns>List of variable names found (e.g., {{variable_name}})</returns>
    List<string> ExtractVariables(string content);

    /// <summary>
    /// Validate that all required variables have values
    /// </summary>
    /// <param name="content">Content to check</param>
    /// <param name="variables">Provided variables</param>
    /// <returns>List of missing variable names</returns>
    List<string> ValidateVariables(string content, Dictionary<string, string> variables);
}
