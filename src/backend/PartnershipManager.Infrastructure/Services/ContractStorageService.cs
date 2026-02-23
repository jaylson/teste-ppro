// F4-SVC-001: ContractStorageService Implementation
// File: src/backend/PartnershipManager.Infrastructure/Services/ContractStorageService.cs
// Author: GitHub Copilot
// Date: 23/02/2026

using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces.Services;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Local-disk implementation of IContractStorageService.
/// Files are stored at: {BaseStoragePath}/contracts/{contractId}/v{versionNumber}.{ext}
/// TODO: Replace with S3/Blob implementation for production.
/// </summary>
public class ContractStorageService : IContractStorageService
{
    private readonly string _basePath;
    private readonly ILogger<ContractStorageService> _logger;

    public ContractStorageService(IConfiguration configuration, ILogger<ContractStorageService> logger)
    {
        _logger = logger;
        // Reads from appsettings: "Storage:BasePath". Falls back to /app/storage
        _basePath = configuration["Storage:BasePath"] ?? "/app/storage";
    }

    /// <inheritdoc />
    public async Task<string> SaveDocxAsync(
        Guid contractId,
        int versionNumber,
        Stream stream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var directory = GetContractDirectory(contractId);
        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine("contracts", contractId.ToString(), $"v{versionNumber}.docx");
        var fullPath = Path.Combine(_basePath, relativePath);

        stream.Position = 0;
        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Saved DOCX version {Version} for contract {ContractId} at {Path}",
            versionNumber, contractId, relativePath);

        return relativePath;
    }

    /// <inheritdoc />
    public async Task<string> SavePdfAsync(
        Guid contractId,
        int versionNumber,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default)
    {
        var directory = GetContractDirectory(contractId);
        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine("contracts", contractId.ToString(), $"v{versionNumber}.pdf");
        var fullPath = Path.Combine(_basePath, relativePath);

        await File.WriteAllBytesAsync(fullPath, pdfBytes, cancellationToken);

        _logger.LogInformation("Saved PDF version {Version} for contract {ContractId} at {Path}",
            versionNumber, contractId, relativePath);

        return relativePath;
    }

    /// <inheritdoc />
    public Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(_basePath, filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Contract file not found: {filePath}", fullPath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public async Task<string> ComputeHashAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var originalPosition = stream.CanSeek ? stream.Position : 0;

        stream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

        if (stream.CanSeek)
            stream.Position = originalPosition;

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <inheritdoc />
    public string GetContentType(DocumentFileType fileType) => fileType switch
    {
        DocumentFileType.Pdf  => "application/pdf",
        DocumentFileType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _                     => "application/octet-stream"
    };

    // ─────────────────────────────────────────────
    private string GetContractDirectory(Guid contractId) =>
        Path.Combine(_basePath, "contracts", contractId.ToString());
}
