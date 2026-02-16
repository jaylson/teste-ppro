// F3-BLD-BE-001: Contract Generation Service
// File: src/backend/PartnershipManager.Infrastructure/Services/ContractGenerationService.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using Microsoft.Extensions.Logging;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Service for generating contracts from templates with variable substitution and PDF output
/// </summary>
public class ContractGenerationService : IContractGenerationService
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractTemplateRepository _templateRepository;
    private readonly IClauseRepository _clauseRepository;
    private readonly ILogger<ContractGenerationService> _logger;

    public ContractGenerationService(
        IContractRepository contractRepository,
        IContractTemplateRepository templateRepository,
        IClauseRepository clauseRepository,
        ILogger<ContractGenerationService> logger)
    {
        _contractRepository = contractRepository;
        _templateRepository = templateRepository;
        _clauseRepository = clauseRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateContractContentAsync(
        Contract contract,
        ContractTemplate template,
        List<Clause> clauses,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating contract content for Contract ID: {ContractId}, Template: {TemplateName}", 
                contract.Id, template.Name);

            // Validate inputs
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            variables ??= new Dictionary<string, string>();

            // Add default variables
            AddDefaultVariables(variables, contract);

            // Validate template variables
            var missingVars = ContractTemplateEngine.ValidateVariables(template.Content, variables);
            if (missingVars.Any())
            {
                _logger.LogWarning("Missing variables in template: {MissingVariables}", string.Join(", ", missingVars));
                // We'll continue but log warning
            }

            // Substitute variables in template
            var processedTemplate = ContractTemplateEngine.SubstituteVariables(
                template.Content,
                variables,
                throwOnMissing: false);

            // Process clauses
            var processedClauses = new List<string>();
            if (clauses != null && clauses.Any())
            {
                foreach (var clause in clauses.OrderBy(c => c.CreatedAt))
                {
                    // Validate clause variables
                    var clauseMissingVars = ContractTemplateEngine.ValidateVariables(clause.Content, variables);
                    if (clauseMissingVars.Any())
                    {
                        _logger.LogWarning("Missing variables in clause {ClauseCode}: {MissingVariables}", 
                            clause.Code, string.Join(", ", clauseMissingVars));
                    }

                    // Substitute variables in clause
                    var processedClause = ContractTemplateEngine.SubstituteVariables(
                        clause.Content,
                        variables,
                        throwOnMissing: false);

                    processedClauses.Add(processedClause);
                }
            }

            // Merge clauses into template
            var mergedContent = ContractTemplateEngine.MergeClauses(
                processedTemplate,
                processedClauses,
                "{{CLAUSES}}");

            // Sanitize HTML
            var sanitizedContent = ContractTemplateEngine.SanitizeHtml(mergedContent);

            // Generate complete HTML document
            var metadata = new Dictionary<string, string>
            {
                { "Tipo de Contrato", contract.ContractType.ToDisplayName() },
                { "Status", contract.Status.ToDisplayName() }
            };

            if (contract.ContractDate.HasValue)
            {
                metadata["Data do Contrato"] = contract.ContractDate.Value.ToString("dd/MM/yyyy");
            }

            if (contract.ExpirationDate.HasValue)
            {
                metadata["Data de Expiração"] = contract.ExpirationDate.Value.ToString("dd/MM/yyyy");
            }

            var htmlDocument = ContractTemplateEngine.GenerateHtmlDocument(
                contract.Title,
                sanitizedContent,
                metadata);

            _logger.LogInformation("Contract content generated successfully for Contract ID: {ContractId}", contract.Id);

            return htmlDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating contract content for Contract ID: {ContractId}", contract.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> GenerateContractPdfAsync(
        Contract contract,
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating PDF for Contract ID: {ContractId}", contract.Id);

            // Configure QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;

            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Element(c => ComposeHeader(c, contract));
                        page.Content().Element(c => ComposeContent(c, htmlContent));
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                });

                _logger.LogInformation("PDF generated successfully for Contract ID: {ContractId}", contract.Id);
                return document.GeneratePdf();
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for Contract ID: {ContractId}", contract.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(string HtmlContent, byte[] PdfBytes)> GenerateCompleteContractAsync(
        Guid contractId,
        Guid templateId,
        List<Guid> clauseIds,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating complete contract for Contract ID: {ContractId}", contractId);

            // Load contract
            var contract = await _contractRepository.GetWithDetailsAsync(contractId, Guid.Empty);
            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {contractId} not found");

            // Load template
            var template = await _templateRepository.GetByIdAsync(templateId, contract.ClientId);
            if (template == null)
                throw new InvalidOperationException($"Template with ID {templateId} not found");

            // Load clauses
            var clauses = new List<Clause>();
            if (clauseIds != null && clauseIds.Any())
            {
                foreach (var clauseId in clauseIds)
                {
                    var clause = await _clauseRepository.GetByIdAsync(clauseId, contract.ClientId);
                    if (clause != null)
                    {
                        clauses.Add(clause);
                    }
                    else
                    {
                        _logger.LogWarning("Clause with ID {ClauseId} not found, skipping", clauseId);
                    }
                }
            }

            // Generate HTML content
            var htmlContent = await GenerateContractContentAsync(
                contract,
                template,
                clauses,
                variables,
                cancellationToken);

            // Generate PDF
            var pdfBytes = await GenerateContractPdfAsync(
                contract,
                htmlContent,
                cancellationToken);

            _logger.LogInformation("Complete contract generated successfully for Contract ID: {ContractId}", contractId);

            return (htmlContent, pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating complete contract for Contract ID: {ContractId}", contractId);
            throw;
        }
    }

    /// <inheritdoc />
    public List<string> ExtractVariables(string content)
    {
        return ContractTemplateEngine.ExtractVariables(content);
    }

    /// <inheritdoc />
    public List<string> ValidateVariables(string content, Dictionary<string, string> variables)
    {
        return ContractTemplateEngine.ValidateVariables(content, variables);
    }

    #region Private Helper Methods

    /// <summary>
    /// Add default variables available to all contracts
    /// </summary>
    private void AddDefaultVariables(Dictionary<string, string> variables, Contract contract)
    {
        // Add if not already present
        variables.TryAdd("CONTRACT_ID", contract.Id.ToString());
        variables.TryAdd("CONTRACT_TITLE", contract.Title);
            variables.TryAdd("CONTRACT_TYPE", contract.ContractType.ToDisplayName());
            variables.TryAdd("CONTRACT_STATUS", contract.Status.ToDisplayName());

        if (contract.ContractDate.HasValue)
        {
            variables.TryAdd("CONTRACT_DATE", contract.ContractDate.Value.ToString("dd/MM/yyyy"));
        }

        if (contract.ExpirationDate.HasValue)
        {
            variables.TryAdd("EXPIRATION_DATE", contract.ExpirationDate.Value.ToString("dd/MM/yyyy"));
        }

        if (!string.IsNullOrEmpty(contract.Description))
        {
            variables.TryAdd("CONTRACT_DESCRIPTION", contract.Description);
        }
    }

    /// <summary>
    /// Compose PDF header with contract info
    /// </summary>
    private void ComposeHeader(IContainer container, Contract contract)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("PARTNERSHIP MANAGER")
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Blue.Medium);
                
                column.Item().Text("Sistema de Gestão de Parcerias")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken2);
            });

            row.ConstantItem(150).AlignRight().Column(column =>
            {
                column.Item().Text(contract.Title)
                    .FontSize(14)
                    .Bold();
                
                column.Item().Text($"Tipo: {contract.ContractType.ToDisplayName()}")
                    .FontSize(9);
                
                column.Item().Text($"Status: {contract.Status.ToDisplayName()}")
                    .FontSize(9);
            });
        });
    }

    /// <summary>
    /// Compose PDF content from HTML
    /// </summary>
    private void ComposeContent(IContainer container, string htmlContent)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Spacing(5);

            // Simple HTML to text conversion (basic implementation)
            // In production, use a proper HTML-to-PDF renderer
            var textContent = StripHtml(htmlContent);
            var paragraphs = textContent.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var paragraph in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph))
                {
                    column.Item().Text(paragraph.Trim())
                        .FontSize(11)
                        .LineHeight(1.5f);
                }
            }
        });
    }

    /// <summary>
    /// Strip HTML tags (basic implementation)
    /// </summary>
    private string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Basic HTML stripping - in production, use HtmlAgilityPack or similar
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
        text = System.Net.WebUtility.HtmlDecode(text);
        
        return text;
    }

    #endregion
}
