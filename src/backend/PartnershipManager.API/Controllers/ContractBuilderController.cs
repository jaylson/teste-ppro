// F3-BLD-BE-002: Contract Builder Controller
// File: src/backend/PartnershipManager.API/Controllers/ContractBuilderController.cs
// Author: GitHub Copilot
// Date: 13/02/2026

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Application.Features.Contracts.Models;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Domain.Interfaces.Services;
using PartnershipManager.Infrastructure.Services;
using System.Collections.Concurrent;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// API endpoints for Contract Builder - 5-step wizard workflow
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ContractBuilderController : ControllerBase
{
    // In-memory session storage (in production, use Redis or database)
    private static readonly ConcurrentDictionary<Guid, BuilderSession> _sessions = new();
    
    private readonly IContractTemplateRepository _templateRepository;
    private readonly IClauseRepository _clauseRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IContractGenerationService _generationService;
    private readonly IContractVersionService _versionService;
    private readonly ILogger<ContractBuilderController> _logger;

    public ContractBuilderController(
        IContractTemplateRepository templateRepository,
        IClauseRepository clauseRepository,
        IContractRepository contractRepository,
        IContractGenerationService generationService,
        IContractVersionService versionService,
        ILogger<ContractBuilderController> logger)
    {
        _templateRepository = templateRepository;
        _clauseRepository = clauseRepository;
        _contractRepository = contractRepository;
        _generationService = generationService;
        _versionService = versionService;
        _logger = logger;
    }

    #region Step 1: Start Session / Select Template

    /// <summary>
    /// Start a new contract builder session (Step 1: Select Template)
    /// </summary>
    /// <param name="request">Template and company info</param>
    /// <returns>Builder session initialized</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(BuilderSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartBuilder([FromBody] StartBuilderRequest request)
    {
        try
        {
            // CompanyId is required, but TemplateId is optional
            if (request.CompanyId == Guid.Empty)
            {
                return BadRequest("CompanyId é obrigatório");
            }

            var clientId = HttpContext.GetRequiredClientId();
            
            // Load template if provided
            ContractTemplate? template = null;
            if (request.TemplateId.HasValue && request.TemplateId != Guid.Empty)
            {
                _logger.LogInformation("Starting builder session for Company: {CompanyId}, Template: {TemplateId}", 
                    request.CompanyId, request.TemplateId);

                template = await _templateRepository.GetByIdAsync(request.TemplateId.Value, clientId);
                if (template == null)
                {
                    return NotFound($"Template with ID {request.TemplateId} not found");
                }

                if (!template.IsActive)
                {
                    return BadRequest("Selected template is not active");
                }
            }
            else
            {
                _logger.LogInformation("Starting blank builder session for Company: {CompanyId}", request.CompanyId);
            }

            // Create session
            var session = new BuilderSession
            {
                SessionId = Guid.NewGuid(),
                ClientId = clientId,
                CompanyId = request.CompanyId,
                TemplateId = request.TemplateId,
                TemplateName = template?.Name ?? "Contrato em Branco",
                Title = request.Title ?? (template != null ? $"{template.Name} - {DateTime.UtcNow:yyyy-MM-dd}" : $"Novo Contrato - {DateTime.UtcNow:yyyy-MM-dd}"),
                CurrentStep = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            // Store session
            _sessions[session.SessionId] = session;

            _logger.LogInformation("Builder session created: {SessionId}", session.SessionId);

            var response = new BuilderSessionResponse
            {
                SessionId = session.SessionId,
                CompanyId = session.CompanyId,
                TemplateId = session.TemplateId,
                TemplateName = session.TemplateName,
                TemplateType = template?.TemplateType,
                Title = session.Title,
                CurrentStep = session.CurrentStep,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting builder session");
            return StatusCode(500, "An error occurred while starting the builder session");
        }
    }

    #endregion

    #region Step 2: Add Parties

    /// <summary>
    /// Add parties/signers to contract (Step 2)
    /// </summary>
    /// <param name="request">Parties information</param>
    /// <returns>Updated session with parties</returns>
    [HttpPost("parties")]
    [ProducesResponseType(typeof(PartiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddParties([FromBody] AddPartiesRequest request)
    {
        try
        {
            if (!_sessions.TryGetValue(request.SessionId, out var session))
            {
                return NotFound($"Session {request.SessionId} not found or expired");
            }

            if (session.IsExpired())
            {
                _sessions.TryRemove(request.SessionId, out _);
                return BadRequest("Session has expired");
            }

            // Validate parties
            if (request.Parties == null || !request.Parties.Any())
            {
                return BadRequest("At least one party is required");
            }

            foreach (var party in request.Parties)
            {
                if (string.IsNullOrWhiteSpace(party.PartyName))
                {
                    return BadRequest("Party name is required");
                }
                if (string.IsNullOrWhiteSpace(party.PartyEmail))
                {
                    return BadRequest("Party email is required");
                }
            }

            // Update session
            session.Parties = request.Parties;
            session.Touch();
            session.SetStep(2);

            _logger.LogInformation("Added {Count} parties to session {SessionId}", 
                request.Parties.Count, request.SessionId);

            var response = new PartiesResponse
            {
                SessionId = session.SessionId,
                Parties = session.Parties,
                CurrentStep = session.CurrentStep
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding parties to session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while adding parties");
        }
    }

    #endregion

    #region Step 3: Select Clauses

    /// <summary>
    /// Select clauses for contract (Step 3)
    /// </summary>
    /// <param name="request">Clause selections</param>
    /// <returns>Updated session with clauses</returns>
    [HttpPost("clauses")]
    [ProducesResponseType(typeof(ClausesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectClauses([FromBody] SelectClausesRequest request)
    {
        try
        {
            if (!_sessions.TryGetValue(request.SessionId, out var session))
            {
                return NotFound($"Session {request.SessionId} not found or expired");
            }

            if (session.IsExpired())
            {
                _sessions.TryRemove(request.SessionId, out _);
                return BadRequest("Session has expired");
            }

            // Validate clauses exist
            foreach (var clauseSelection in request.Clauses)
            {
                var clause = await _clauseRepository.GetByIdAsync(clauseSelection.ClauseId, session.ClientId);
                if (clause == null)
                {
                    return NotFound($"Clause {clauseSelection.ClauseId} not found");
                }
            }

            // Update session
            session.Clauses = request.Clauses;
            session.Touch();
            session.SetStep(3);

            _logger.LogInformation("Added {Count} clauses to session {SessionId}", 
                request.Clauses.Count, request.SessionId);

            var response = new ClausesResponse
            {
                SessionId = session.SessionId,
                Clauses = session.Clauses,
                CurrentStep = session.CurrentStep
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting clauses for session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while selecting clauses");
        }
    }

    #endregion

    #region Step 4: Fill Data (Variables)

    /// <summary>
    /// Fill contract data and variables (Step 4)
    /// </summary>
    /// <param name="request">Variables and metadata</param>
    /// <returns>Updated session with data</returns>
    [HttpPost("data")]
    [ProducesResponseType(typeof(DataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FillData([FromBody] FillDataRequest request)
    {
        try
        {
            if (!_sessions.TryGetValue(request.SessionId, out var session))
            {
                return NotFound($"Session {request.SessionId} not found or expired");
            }

            if (session.IsExpired())
            {
                _sessions.TryRemove(request.SessionId, out _);
                return BadRequest("Session has expired");
            }

            // Load template to get required variables (or use blank template)
            ContractTemplate? template = null;
            if (session.TemplateId.HasValue)
            {
                template = await _templateRepository.GetByIdAsync(session.TemplateId.Value, session.ClientId);
                if (template == null)
                {
                    return NotFound("Template not found");
                }
            }
            else
            {
                template = CreateBlankTemplate(session.ClientId, session.CompanyId);
            }

            var templateContent = template.Content;

            // Extract required variables
            var requiredVariables = _generationService.ExtractVariables(templateContent)
                .Where(v => !string.Equals(v, "CLAUSES", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            // Add clause variables
            foreach (var clauseSelection in session.Clauses)
            {
                var clause = await _clauseRepository.GetByIdAsync(clauseSelection.ClauseId, session.ClientId);
                if (clause != null)
                {
                    var clauseVars = _generationService.ExtractVariables(clause.Content);
                    requiredVariables.AddRange(clauseVars);
                }
            }

            requiredVariables = requiredVariables.Distinct().ToList();

            // Check for missing variables
            var missingVariables = _generationService.ValidateVariables(
                templateContent, 
                request.Variables ?? new Dictionary<string, string>())
                .Where(v => !string.Equals(v, "CLAUSES", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Update session
            session.Variables = request.Variables ?? new Dictionary<string, string>();
            session.ContractDate = request.ContractDate;
            session.ExpirationDate = request.ExpirationDate;
            session.Description = request.Description;
            session.Notes = request.Notes;
            session.Touch();
            session.SetStep(4);

            _logger.LogInformation("Filled data for session {SessionId}, {MissingCount} missing variables", 
                request.SessionId, missingVariables.Count);

            var response = new DataResponse
            {
                SessionId = session.SessionId,
                Variables = session.Variables,
                ContractDate = session.ContractDate,
                ExpirationDate = session.ExpirationDate,
                Description = session.Description,
                Notes = session.Notes,
                RequiredVariables = requiredVariables,
                MissingVariables = missingVariables,
                CurrentStep = session.CurrentStep
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filling data for session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while filling data");
        }
    }

    #endregion

    #region Step 5: Preview & Generate

    /// <summary>
    /// Preview contract before generation (Step 5)
    /// </summary>
    /// <param name="request">Session to preview</param>
    /// <returns>HTML preview of contract</returns>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(PreviewContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewContract([FromBody] PreviewContractRequest request)
    {
        try
        {
            if (!_sessions.TryGetValue(request.SessionId, out var session))
            {
                return NotFound($"Session {request.SessionId} not found or expired");
            }

            if (session.IsExpired())
            {
                _sessions.TryRemove(request.SessionId, out _);
                return BadRequest("Session has expired");
            }

            var validationErrors = new List<string>();

            // Validate session data
            if (!session.Parties.Any())
            {
                validationErrors.Add("At least one party is required");
            }

            // Load template and clauses
            ContractTemplate? template = null;
            if (session.TemplateId.HasValue)
            {
                template = await _templateRepository.GetByIdAsync(session.TemplateId.Value, session.ClientId);
                if (template == null)
                {
                    return NotFound("Template not found");
                }
            }
            else
            {
                template = CreateBlankTemplate(session.ClientId, session.CompanyId);
            }

            var clauses = new List<Clause>();
            foreach (var clauseSelection in session.Clauses)
            {
                var clause = await _clauseRepository.GetByIdAsync(clauseSelection.ClauseId, session.ClientId);
                if (clause != null)
                {
                    clauses.Add(clause);
                }
            }

            // Create temporary contract for preview
            var tempContract = Contract.Create(
                clientId: session.ClientId,
                companyId: session.CompanyId,
                title: session.Title,
                contractType: template.TemplateType,
                templateId: session.TemplateId,
                contractDate: session.ContractDate,
                expirationDate: session.ExpirationDate,
                description: session.Description,
                createdBy: Guid.Empty);

            // Generate preview HTML
            var htmlPreview = await _generationService.GenerateContractContentAsync(
                tempContract,
                template,
                clauses,
                session.Variables);

            // Cache preview
            session.HtmlPreview = htmlPreview;
            session.Touch();
            session.SetStep(5);

            _logger.LogInformation("Generated preview for session {SessionId}", request.SessionId);

            var response = new PreviewContractResponse
            {
                SessionId = session.SessionId,
                HtmlPreview = htmlPreview,
                IsValid = !validationErrors.Any(),
                ValidationErrors = validationErrors,
                CurrentStep = session.CurrentStep
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview for session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while generating preview");
        }
    }

    /// <summary>
    /// Generate final contract document (Step 5 - Final)
    /// </summary>
    /// <param name="request">Session to generate</param>
    /// <returns>Generated contract information</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateContract([FromBody] GenerateContractRequest request)
    {
        try
        {
            if (!_sessions.TryGetValue(request.SessionId, out var session))
            {
                return NotFound($"Session {request.SessionId} not found or expired");
            }

            if (session.IsExpired())
            {
                _sessions.TryRemove(request.SessionId, out _);
                return BadRequest("Session has expired");
            }

            // Validate session
            if (!session.Parties.Any())
            {
                return BadRequest("At least one party is required");
            }

            // Load template
            ContractTemplate? template = null;
            if (session.TemplateId.HasValue)
            {
                template = await _templateRepository.GetByIdAsync(session.TemplateId.Value, session.ClientId);
                if (template == null)
                {
                    return NotFound("Template not found");
                }
            }
            else
            {
                template = CreateBlankTemplate(session.ClientId, session.CompanyId);
            }

            // Create contract entity
            var contract = Contract.Create(
                clientId: session.ClientId,
                companyId: session.CompanyId,
                title: session.Title,
                contractType: template.TemplateType,
                templateId: session.TemplateId,
                contractDate: session.ContractDate,
                expirationDate: session.ExpirationDate,
                description: session.Description,
                createdBy: Guid.Empty);

            // Save contract to database
            await _contractRepository.AddAsync(contract);

            // Load clauses
            var clauseIds = session.Clauses.Select(c => c.ClauseId).ToList();

            // Generate complete contract (HTML + PDF)
            var clauses = new List<Clause>();
            foreach (var clauseId in clauseIds)
            {
                var clause = await _clauseRepository.GetByIdAsync(clauseId, session.ClientId);
                if (clause != null)
                {
                    clauses.Add(clause);
                }
            }

            var htmlContent = await _generationService.GenerateContractContentAsync(
                contract,
                template,
                clauses,
                session.Variables);

            var pdfBytes = await _generationService.GenerateContractPdfAsync(
                contract,
                htmlContent);

            // TODO: Save PDF to storage (S3, Azure Blob, etc.)
            // For now, we'll just set a placeholder path
            var documentPath = $"/contracts/{contract.Id}.pdf";
            
            // Update contract with document info
            contract.SetDocument(
                documentPath: documentPath,
                documentSize: pdfBytes.Length,
                documentHash: ComputeHash(pdfBytes));

            if (request.SendForSignature)
            {
                // TODO: Implement TransitionTo method in Contract entity
                // contract.TransitionTo(ContractStatus.SentForSignature, null);
                // TODO: Send to ClickSign
            }

            await _contractRepository.UpdateAsync(contract);

            // Register version 1 generated by the builder
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _versionService.RecordBuilderVersionAsync(
                contractId : contract.Id,
                clientId   : session.ClientId,
                pdfBytes   : pdfBytes,
                createdBy  : userId);
            // Mark session as completed
            session.GeneratedContractId = contract.Id;
            
            // Clean up session after some time (optional)
            // _sessions.TryRemove(request.SessionId, out _);

            _logger.LogInformation("Contract generated: {ContractId} from session {SessionId}", 
                contract.Id, request.SessionId);

            var response = new GenerateContractResponse
            {
                ContractId = contract.Id,
                SessionId = session.SessionId,
                Title = contract.Title,
                Status = contract.Status,
                DocumentPath = contract.DocumentPath,
                DocumentSize = contract.DocumentSize,
                SentForSignature = request.SendForSignature,
                GeneratedAt = DateTime.UtcNow
            };

            return CreatedAtAction("GetById", "Contracts", new { id = contract.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating contract for session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while generating the contract");
        }
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Get current builder session state
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Complete session data</returns>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(CompleteSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSession(Guid sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return NotFound($"Session {sessionId} not found or expired");
        }

        if (session.IsExpired())
        {
            _sessions.TryRemove(sessionId, out _);
            return NotFound("Session has expired");
        }

        session.Touch();

        return Ok(session.ToResponse());
    }

    /// <summary>
    /// Cancel/delete builder session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CancelSession(Guid sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            return NotFound($"Session {sessionId} not found");
        }

        _logger.LogInformation("Builder session cancelled: {SessionId}", sessionId);

        return NoContent();
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Compute SHA-256 hash of byte array
    /// </summary>
    private static string ComputeHash(byte[] data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Build a lightweight blank template for "start from scratch" flows
    /// </summary>
    private static ContractTemplate CreateBlankTemplate(Guid clientId, Guid? companyId)
    {
        return ContractTemplate.Create(
            clientId: clientId,
            name: "Contrato em Branco",
            code: $"BLANK-{Guid.NewGuid():N}",
            templateType: ContractTemplateType.Other,
            content: "<div>{{CLAUSES}}</div>",
            description: "Template em branco gerado automaticamente",
            companyId: companyId,
            defaultStatus: ContractStatus.Draft,
            tags: new List<string> { "blank" },
            createdBy: null);
    }

    #endregion
}
