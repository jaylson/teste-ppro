namespace PartnershipManager.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }
    
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} com identificador '{key}' não foi encontrado(a).")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException() : base("Uma ou mais validações falharam.")
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }
    
    public ValidationException(string property, string message) : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { property, new[] { message } }
        };
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException() : base("Acesso não autorizado.") { }
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException() : base("Você não tem permissão para realizar esta ação.") { }
    public ForbiddenException(string message) : base(message) { }
}

public class BusinessRuleException : DomainException
{
    public string RuleName { get; }
    
    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}
