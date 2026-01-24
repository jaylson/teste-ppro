using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;

namespace PartnershipManager.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? TradingName { get; private set; }
    public string Document { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Settings { get; private set; }
    public ClientStatus Status { get; private set; }
    
    // Navigation properties
    public ICollection<Company> Companies { get; private set; } = new List<Company>();
    public ICollection<User> Users { get; private set; } = new List<User>();
    
    // Calculated
    public string DocumentFormatted => DocumentType == DocumentType.Cnpj 
        ? FormatCnpj(Document) 
        : FormatCpf(Document);
    
    private Client() { }
    
    public static Client Create(
        string name,
        string document,
        DocumentType documentType,
        string email,
        string? tradingName = null,
        string? phone = null)
    {
        ValidateName(name);
        ValidateDocument(document, documentType);
        ValidateEmail(email);
        
        return new Client
        {
            Name = name.Trim(),
            TradingName = tradingName?.Trim(),
            Document = NormalizeDocument(document),
            DocumentType = documentType,
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            Status = ClientStatus.Active
        };
    }
    
    public void UpdateBasicInfo(string name, string? tradingName, string? phone, string? logoUrl)
    {
        ValidateName(name);
        Name = name.Trim();
        TradingName = tradingName?.Trim();
        Phone = phone?.Trim();
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateEmail(string email)
    {
        ValidateEmail(email);
        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateSettings(string settings)
    {
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        if (Status == ClientStatus.Active)
            throw new DomainException("Client is already active");
            
        Status = ClientStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Suspend()
    {
        if (Status == ClientStatus.Suspended)
            throw new DomainException("Client is already suspended");
            
        Status = ClientStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        if (Status == ClientStatus.Inactive)
            throw new DomainException("Client is already inactive");
            
        Status = ClientStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Validations
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name is required");
            
        if (name.Length < 3)
            throw new DomainException("Client name must have at least 3 characters");
            
        if (name.Length > 200)
            throw new DomainException("Client name cannot exceed 200 characters");
    }
    
    private static void ValidateDocument(string document, DocumentType documentType)
    {
        if (string.IsNullOrWhiteSpace(document))
            throw new DomainException("Document is required");
        
        var normalized = NormalizeDocument(document);
        
        if (documentType == DocumentType.Cnpj)
        {
            if (normalized.Length != 14)
                throw new DomainException("CNPJ must have 14 digits");
                
            if (!IsValidCnpj(normalized))
                throw new DomainException("Invalid CNPJ");
        }
        else if (documentType == DocumentType.Cpf)
        {
            if (normalized.Length != 11)
                throw new DomainException("CPF must have 11 digits");
                
            if (!IsValidCpf(normalized))
                throw new DomainException("Invalid CPF");
        }
    }
    
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
            
        if (email.Length > 255)
            throw new DomainException("Email cannot exceed 255 characters");
            
        if (!email.Contains("@") || !email.Contains("."))
            throw new DomainException("Invalid email format");
    }
    
    // Helpers
    private static string NormalizeDocument(string document)
    {
        return new string(document.Where(char.IsDigit).ToArray());
    }
    
    private static string FormatCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return cnpj;
            
        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }
    
    private static string FormatCpf(string cpf)
    {
        if (cpf.Length != 11)
            return cpf;
            
        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }
    
    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;
            
        // Verificar se todos os dígitos são iguais
        if (cnpj.Distinct().Count() == 1)
            return false;
        
        // Validação dos dígitos verificadores
        var tempCnpj = cnpj.Substring(0, 12);
        var sum = 0;
        var pos = 5;
        
        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(tempCnpj[i].ToString()) * pos;
            pos = pos == 2 ? 9 : pos - 1;
        }
        
        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;
        
        tempCnpj += digit;
        sum = 0;
        pos = 6;
        
        for (int i = 0; i < 13; i++)
        {
            sum += int.Parse(tempCnpj[i].ToString()) * pos;
            pos = pos == 2 ? 9 : pos - 1;
        }
        
        remainder = sum % 11;
        digit = remainder < 2 ? 0 : 11 - remainder;
        
        return cnpj.EndsWith(tempCnpj.Substring(12) + digit.ToString());
    }
    
    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;
            
        // Verificar se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1)
            return false;
        
        // Validação dos dígitos verificadores
        var tempCpf = cpf.Substring(0, 9);
        var sum = 0;
        
        for (int i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * (10 - i);
        
        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;
        
        tempCpf += digit;
        sum = 0;
        
        for (int i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * (11 - i);
        
        remainder = sum % 11;
        digit = remainder < 2 ? 0 : 11 - remainder;
        
        return cpf.EndsWith(tempCpf.Substring(9) + digit.ToString());
    }
}
