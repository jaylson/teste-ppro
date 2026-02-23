// F4-REP-001: ContractVersionRepository Implementation
// File: src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/Contract/ContractVersionRepository.cs
// Author: GitHub Copilot
// Date: 23/02/2026

using System.Data;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper repository for contract version records
/// </summary>
public class ContractVersionRepository : IContractVersionRepository
{
    private readonly DapperContext _context;

    public ContractVersionRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        cv.id             AS Id,
        cv.contract_id    AS ContractId,
        cv.version_number AS VersionNumber,
        cv.file_path      AS FilePath,
        cv.file_size      AS FileSize,
        cv.file_hash      AS FileHash,
        cv.file_type      AS FileType,
        cv.source         AS Source,
        cv.notes          AS Notes,
        cv.created_at     AS CreatedAt,
        cv.created_by     AS CreatedBy";

    public async Task<IEnumerable<ContractVersion>> GetByContractAsync(Guid contractId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM contract_versions cv
            WHERE cv.contract_id = @ContractId
            ORDER BY cv.version_number DESC";

        var rows = await Connection.QueryAsync<ContractVersionRow>(sql,
            new { ContractId = contractId.ToString() },
            Transaction);

        return rows.Select(Map);
    }

    public async Task<ContractVersion?> GetByIdAsync(Guid versionId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM contract_versions cv
            WHERE cv.id = @Id
            LIMIT 1";

        var row = await Connection.QueryFirstOrDefaultAsync<ContractVersionRow>(sql,
            new { Id = versionId.ToString() },
            Transaction);

        return row is null ? null : Map(row);
    }

    public async Task<ContractVersion?> GetByVersionNumberAsync(Guid contractId, int versionNumber)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM contract_versions cv
            WHERE cv.contract_id = @ContractId AND cv.version_number = @VersionNumber
            LIMIT 1";

        var row = await Connection.QueryFirstOrDefaultAsync<ContractVersionRow>(sql,
            new { ContractId = contractId.ToString(), VersionNumber = versionNumber },
            Transaction);

        return row is null ? null : Map(row);
    }

    public async Task<ContractVersion?> GetLatestAsync(Guid contractId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM contract_versions cv
            WHERE cv.contract_id = @ContractId
            ORDER BY cv.version_number DESC
            LIMIT 1";

        var row = await Connection.QueryFirstOrDefaultAsync<ContractVersionRow>(sql,
            new { ContractId = contractId.ToString() },
            Transaction);

        return row is null ? null : Map(row);
    }

    public async Task AddAsync(ContractVersion version)
    {
        var sql = @"
            INSERT INTO contract_versions
                (id, contract_id, version_number, file_path, file_size, file_hash,
                 file_type, source, notes, created_at, created_by)
            VALUES
                (@Id, @ContractId, @VersionNumber, @FilePath, @FileSize, @FileHash,
                 @FileType, @Source, @Notes, @CreatedAt, @CreatedBy)";

        await Connection.ExecuteAsync(sql, new
        {
            Id           = version.Id.ToString(),
            ContractId   = version.ContractId.ToString(),
            version.VersionNumber,
            version.FilePath,
            version.FileSize,
            version.FileHash,
            FileType     = version.FileType.ToString().ToLowerInvariant(),
            Source       = version.Source.ToString().ToLowerInvariant(),
            version.Notes,
            version.CreatedAt,
            version.CreatedBy
        }, Transaction);
    }

    // ─────────────────────────────────────────────────────────────────
    // Flat query row — avoids reflection issues with private setters
    // Note: Id and ContractId are Guid because MySqlConnector returns CHAR(36)
    // as System.Guid objects; using string causes InvalidCastException.
    private class ContractVersionRow
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; }
        public int VersionNumber { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public string? FileHash { get; set; }
        public string FileType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    private static ContractVersion Map(ContractVersionRow row)
    {
        var fileType = Enum.Parse<DocumentFileType>(row.FileType, ignoreCase: true);
        var source   = Enum.Parse<ContractVersionSource>(row.Source, ignoreCase: true);

        return ContractVersion.Reconstitute(
            id            : row.Id,
            contractId    : row.ContractId,
            versionNumber : row.VersionNumber,
            filePath      : row.FilePath,
            fileType      : fileType,
            source        : source,
            fileSize      : row.FileSize,
            fileHash      : row.FileHash,
            notes         : row.Notes,
            createdBy     : row.CreatedBy,
            createdAt     : row.CreatedAt
        );
    }
}
