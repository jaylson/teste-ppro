using System.Data;
using System.Text;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class ShareholderRepository : IShareholderRepository
{
    private readonly DapperContext _context;

    public ShareholderRepository(DapperContext context)
    {
        _context = context;
    }

    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    public async Task<(IEnumerable<Shareholder> Items, int Total)> GetPagedAsync(
        Guid clientId,
        Guid? companyId,
        int page,
        int pageSize,
        string? search = null,
        string? type = null,
        string? status = null)
    {
        var baseQuery = new StringBuilder(@"FROM shareholders sh
                    JOIN companies c ON c.id = sh.company_id
                    WHERE sh.is_deleted = 0
                      AND sh.client_id = @ClientId");

        var parameters = new DynamicParameters();
        parameters.Add("ClientId", clientId.ToString());

        if (companyId.HasValue)
        {
            baseQuery.Append(" AND sh.company_id = @CompanyId");
            parameters.Add("CompanyId", companyId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            baseQuery.Append(" AND (LOWER(sh.name) LIKE @Search OR sh.document LIKE @Search OR LOWER(sh.email) LIKE @Search)");
            parameters.Add("Search", $"%{search.ToLower()}%");
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            baseQuery.Append(" AND sh.type = @Type");
            parameters.Add("Type", type);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            baseQuery.Append(" AND sh.status = @Status");
            parameters.Add("Status", status);
        }

        var countSql = $"SELECT COUNT(1) {baseQuery}";
        var total = await Connection.ExecuteScalarAsync<int>(countSql, parameters, Transaction);

        baseQuery.Append(" ORDER BY sh.created_at DESC LIMIT @Offset, @PageSize");
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var sql = $@"SELECT 
                        sh.id AS Id,
                        sh.client_id AS ClientId,
                        sh.company_id AS CompanyId,
                        sh.name AS Name,
                        sh.document AS Document,
                        sh.document_type AS DocumentType,
                        sh.email AS Email,
                        sh.phone AS Phone,
                        sh.type AS Type,
                        sh.status AS Status,
                        sh.notes AS Notes,
                        sh.address_street AS AddressStreet,
                        sh.address_number AS AddressNumber,
                        sh.address_complement AS AddressComplement,
                        sh.address_zip_code AS AddressZipCode,
                        sh.address_city AS AddressCity,
                        sh.address_state AS AddressState,
                        sh.marital_status AS MaritalStatus,
                        sh.gender AS Gender,
                        sh.birth_date AS BirthDate,
                        sh.created_at AS CreatedAt,
                        sh.updated_at AS UpdatedAt,
                        sh.created_by AS CreatedBy,
                        sh.updated_by AS UpdatedBy,
                        sh.is_deleted AS IsDeleted,
                        sh.deleted_at AS DeletedAt,
                        c.name AS CompanyName
                    {baseQuery}";

        var rows = await Connection.QueryAsync<dynamic>(sql, parameters, Transaction);
        var items = rows.Select(MapToShareholder);

        return (items, total);
    }

    public async Task<Shareholder?> GetByIdAsync(Guid id, Guid clientId)
    {
        var sql = @"SELECT 
                        sh.id AS Id,
                        sh.client_id AS ClientId,
                        sh.company_id AS CompanyId,
                        sh.name AS Name,
                        sh.document AS Document,
                        sh.document_type AS DocumentType,
                        sh.email AS Email,
                        sh.phone AS Phone,
                        sh.type AS Type,
                        sh.status AS Status,
                        sh.notes AS Notes,
                        sh.address_street AS AddressStreet,
                        sh.address_number AS AddressNumber,
                        sh.address_complement AS AddressComplement,
                        sh.address_zip_code AS AddressZipCode,
                        sh.address_city AS AddressCity,
                        sh.address_state AS AddressState,
                        sh.marital_status AS MaritalStatus,
                        sh.gender AS Gender,
                        sh.birth_date AS BirthDate,
                        sh.created_at AS CreatedAt,
                        sh.updated_at AS UpdatedAt,
                        sh.created_by AS CreatedBy,
                        sh.updated_by AS UpdatedBy,
                        sh.is_deleted AS IsDeleted,
                        sh.deleted_at AS DeletedAt,
                        c.name AS CompanyName
                    FROM shareholders sh
                    JOIN companies c ON c.id = sh.company_id
                    WHERE sh.id = @Id AND sh.client_id = @ClientId AND sh.is_deleted = 0";

        var shareholder = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);

        return shareholder == null ? null : MapToShareholder(shareholder);
    }

    public async Task<Shareholder?> GetByDocumentAsync(Guid clientId, string document)
    {
        var normalized = new string(document.Where(char.IsDigit).ToArray());

        var sql = @"SELECT 
                        sh.id AS Id,
                        sh.client_id AS ClientId,
                        sh.company_id AS CompanyId,
                        sh.name AS Name,
                        sh.document AS Document,
                        sh.document_type AS DocumentType,
                        sh.email AS Email,
                        sh.phone AS Phone,
                        sh.type AS Type,
                        sh.status AS Status,
                        sh.notes AS Notes,
                        sh.address_street AS AddressStreet,
                        sh.address_number AS AddressNumber,
                        sh.address_complement AS AddressComplement,
                        sh.address_zip_code AS AddressZipCode,
                        sh.address_city AS AddressCity,
                        sh.address_state AS AddressState,
                        sh.marital_status AS MaritalStatus,
                        sh.gender AS Gender,
                        sh.birth_date AS BirthDate,
                        sh.created_at AS CreatedAt,
                        sh.updated_at AS UpdatedAt,
                        sh.created_by AS CreatedBy,
                        sh.updated_by AS UpdatedBy,
                        sh.is_deleted AS IsDeleted,
                        sh.deleted_at AS DeletedAt,
                        c.name AS CompanyName
                    FROM shareholders sh
                    JOIN companies c ON c.id = sh.company_id
                    WHERE sh.client_id = @ClientId
                      AND sh.document = @Document
                      AND sh.is_deleted = 0";

        var shareholder = await Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            Document = normalized
        }, Transaction);

        return shareholder == null ? null : MapToShareholder(shareholder);
    }

    public async Task<bool> DocumentExistsAsync(Guid clientId, string document, Guid? excludeId = null)
    {
        var normalized = new string(document.Where(char.IsDigit).ToArray());

        var sql = @"SELECT COUNT(1) FROM shareholders
                    WHERE client_id = @ClientId
                      AND document = @Document
                      AND is_deleted = 0
                      AND (@ExcludeId IS NULL OR id != @ExcludeId)";

        var count = await Connection.ExecuteScalarAsync<int>(sql, new
        {
            ClientId = clientId.ToString(),
            Document = normalized,
            ExcludeId = excludeId?.ToString()
        }, Transaction);

        return count > 0;
    }

    public async Task AddAsync(Shareholder shareholder)
    {
        var sql = @"INSERT INTO shareholders
                (id, client_id, company_id, name, document, document_type, email, phone, type, status, notes,
                 address_street, address_number, address_complement, address_zip_code, address_city, address_state,
                 marital_status, gender, birth_date, created_at, updated_at, created_by, updated_by, is_deleted)
                VALUES
                (@Id, @ClientId, @CompanyId, @Name, @Document, @DocumentType, @Email, @Phone, @Type, @Status, @Notes,
                 @AddressStreet, @AddressNumber, @AddressComplement, @AddressZipCode, @AddressCity, @AddressState,
                 @MaritalStatus, @Gender, @BirthDate, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy, @IsDeleted)";

        await Connection.ExecuteAsync(sql, new
        {
            Id = shareholder.Id.ToString(),
            ClientId = shareholder.ClientId.ToString(),
            CompanyId = shareholder.CompanyId.ToString(),
            shareholder.Name,
            shareholder.Document,
            DocumentType = shareholder.DocumentType.ToString().ToLower(),
            shareholder.Email,
            shareholder.Phone,
            Type = shareholder.Type.ToString(),
            Status = shareholder.Status.ToString(),
            shareholder.Notes,
            shareholder.AddressStreet,
            shareholder.AddressNumber,
            shareholder.AddressComplement,
            AddressZipCode = shareholder.AddressZipCode,
            AddressCity = shareholder.AddressCity,
            AddressState = shareholder.AddressState,
            MaritalStatus = shareholder.MaritalStatus?.ToString(),
            Gender = shareholder.Gender?.ToString(),
            BirthDate = shareholder.BirthDate,
            shareholder.CreatedAt,
            shareholder.UpdatedAt,
            CreatedBy = shareholder.CreatedBy?.ToString(),
            UpdatedBy = shareholder.UpdatedBy?.ToString(),
            shareholder.IsDeleted
        }, Transaction);
    }

    public async Task UpdateAsync(Shareholder shareholder)
    {
        var sql = @"UPDATE shareholders
                    SET company_id = @CompanyId,
                        name = @Name,
                        document = @Document,
                        document_type = @DocumentType,
                        email = @Email,
                        phone = @Phone,
                        type = @Type,
                        status = @Status,
                        notes = @Notes,
                        address_street = @AddressStreet,
                        address_number = @AddressNumber,
                        address_complement = @AddressComplement,
                        address_zip_code = @AddressZipCode,
                        address_city = @AddressCity,
                        address_state = @AddressState,
                        marital_status = @MaritalStatus,
                        gender = @Gender,
                        birth_date = @BirthDate,
                        updated_at = @UpdatedAt,
                        updated_by = @UpdatedBy
                    WHERE id = @Id AND client_id = @ClientId";

        await Connection.ExecuteAsync(sql, new
        {
            Id = shareholder.Id.ToString(),
            ClientId = shareholder.ClientId.ToString(),
            CompanyId = shareholder.CompanyId.ToString(),
            shareholder.Name,
            shareholder.Document,
            DocumentType = shareholder.DocumentType.ToString().ToLower(),
            shareholder.Email,
            shareholder.Phone,
            Type = shareholder.Type.ToString(),
            Status = shareholder.Status.ToString(),
            shareholder.Notes,
            shareholder.AddressStreet,
            shareholder.AddressNumber,
            shareholder.AddressComplement,
            AddressZipCode = shareholder.AddressZipCode,
            AddressCity = shareholder.AddressCity,
            AddressState = shareholder.AddressState,
            MaritalStatus = shareholder.MaritalStatus?.ToString(),
            Gender = shareholder.Gender?.ToString(),
            BirthDate = shareholder.BirthDate,
            shareholder.UpdatedAt,
            UpdatedBy = shareholder.UpdatedBy?.ToString()
        }, Transaction);
    }

    public async Task SoftDeleteAsync(Guid id, Guid clientId, Guid? deletedBy = null)
    {
        var sql = @"UPDATE shareholders
                    SET is_deleted = 1,
                        deleted_at = @DeletedAt,
                        updated_at = @DeletedAt,
                        updated_by = @DeletedBy
                    WHERE id = @Id AND client_id = @ClientId";

        var deletedAt = DateTime.UtcNow;

        await Connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString(),
            DeletedAt = deletedAt,
            DeletedBy = deletedBy?.ToString()
        }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid clientId)
    {
        var sql = @"SELECT COUNT(1) FROM shareholders WHERE id = @Id AND client_id = @ClientId AND is_deleted = 0";
        var count = await Connection.ExecuteScalarAsync<int>(sql, new
        {
            Id = id.ToString(),
            ClientId = clientId.ToString()
        }, Transaction);
        return count > 0;
    }

    private static Shareholder MapToShareholder(dynamic row)
    {
        // Converter GUIDs - Dapper pode retornar como Guid ou string dependendo do driver
        Guid ParseGuid(object value) => value is Guid g ? g : Guid.Parse(value.ToString()!);
        Guid? ParseNullableGuid(object? value) => value == null ? null : (value is Guid g ? g : Guid.Parse(value.ToString()!));
        
        var shareholder = Shareholder.Create(
            ParseGuid(row.ClientId),
            ParseGuid(row.CompanyId),
            (string)row.Name,
            (string)row.Document,
            Enum.Parse<DocumentType>((string)row.DocumentType, true),
            Enum.Parse<ShareholderType>((string)row.Type, true),
            (string?)row.Email,
            (string?)row.Phone,
            Enum.Parse<ShareholderStatus>((string)row.Status, true),
            (string?)row.Notes,
            (string?)row.AddressStreet,
            (string?)row.AddressNumber,
            (string?)row.AddressComplement,
            (string?)row.AddressZipCode,
            (string?)row.AddressCity,
            (string?)row.AddressState,
            row.MaritalStatus == null ? null : Enum.Parse<MaritalStatus>((string)row.MaritalStatus, true),
            row.Gender == null ? null : Enum.Parse<Gender>((string)row.Gender, true),
            row.BirthDate as DateTime?,
            ParseNullableGuid(row.CreatedBy));

        shareholder.Id = ParseGuid(row.Id);
        shareholder.CreatedAt = (DateTime)row.CreatedAt;
        shareholder.UpdatedAt = (DateTime)row.UpdatedAt;
        shareholder.IsDeleted = row.IsDeleted is bool b ? b : row.IsDeleted == 1;
        shareholder.DeletedAt = row.DeletedAt as DateTime?;
        shareholder.SetCompanyName((string?)row.CompanyName);

        return shareholder;
    }
}
