using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

public class Shareholder : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public ShareholderType Type { get; private set; }
    public ShareholderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? CompanyName { get; private set; }

    public string DocumentFormatted => DocumentType == DocumentType.Cnpj
        ? Convert.ToUInt64(Document).ToString(@"00\.000\.000\/0000\-00")
        : Convert.ToUInt64(Document).ToString(@"000\.000\.000\-00");

    private Shareholder() {}

    public static Shareholder Create(
        Guid clientId,
        Guid companyId,
        string name,
        string document,
        DocumentType documentType,
        ShareholderType type,
        string? email = null,
        string? phone = null,
        ShareholderStatus status = ShareholderStatus.Active,
        string? notes = null,
        Guid? createdBy = null)
    {
        var normalizedDocument = NormalizeDocument(document);

        if (!IsValidDocument(normalizedDocument, documentType))
        {
            throw new ArgumentException("Documento inválido para o tipo informado", nameof(document));
        }

        return new Shareholder
        {
            ClientId = clientId,
            CompanyId = companyId,
            Name = name.Trim(),
            Document = normalizedDocument,
            DocumentType = documentType,
            Type = type,
            Email = email?.Trim(),
            Phone = phone?.Trim(),
            Status = status,
            Notes = notes?.Trim(),
            CompanyName = null,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void UpdateInfo(
        string name,
        string? email,
        string? phone,
        ShareholderType type,
        ShareholderStatus status,
        string? notes,
        Guid? updatedBy = null)
    {
        Name = name.Trim();
        Email = email?.Trim();
        Phone = phone?.Trim();
        Type = type;
        Status = status;
        Notes = notes?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDocument(string document, DocumentType documentType, Guid? updatedBy = null)
    {
        var normalized = NormalizeDocument(document);

        if (!IsValidDocument(normalized, documentType))
        {
            throw new ArgumentException("Documento inválido para o tipo informado", nameof(document));
        }

        Document = normalized;
        DocumentType = documentType;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeCompany(Guid companyId, Guid? updatedBy = null)
    {
        CompanyId = companyId;
        CompanyName = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCompanyName(string? companyName)
    {
        CompanyName = companyName;
    }

    private static string NormalizeDocument(string document)
    {
        return new string(document.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidDocument(string document, DocumentType type)
    {
        return type switch
        {
            DocumentType.Cnpj => IsValidCnpj(document),
            DocumentType.Cpf => IsValidCpf(document),
            _ => false
        };
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return false;
        var clean = new string(cnpj.Where(char.IsDigit).ToArray());
        if (clean.Length != 14) return false;
        if (clean.All(d => d == clean[0])) return false;

        int[] multipliers1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multipliers2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = clean[..12];
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multipliers1[i];

        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;
        tempCnpj += firstDigit;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multipliers2[i];

        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return clean.EndsWith(firstDigit.ToString() + secondDigit.ToString());
    }

    private static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var clean = new string(cpf.Where(char.IsDigit).ToArray());
        if (clean.Length != 11) return false;
        if (clean.All(d => d == clean[0])) return false;

        int[] multipliers1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multipliers2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = clean[..9];
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multipliers1[i];

        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;
        tempCpf += firstDigit;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multipliers2[i];

        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return clean.EndsWith(firstDigit.ToString() + secondDigit.ToString());
    }
}
