namespace PartnershipManager.Application.Common.Models;

/// <summary>
/// Resultado de operação
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }
    
    protected Result(bool isSuccess, string? error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors?.ToList() ?? (error != null ? new List<string> { error } : new List<string>());
    }
    
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors.FirstOrDefault(), errors);
    
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Resultado de operação com valor
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }
    
    protected internal Result(T value, bool isSuccess, string? error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Resultado paginado
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
    
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) 
        => new(Array.Empty<T>(), 0, pageNumber, pageSize);
}

/// <summary>
/// Resposta de API padronizada
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };
    
    public static ApiResponse<T> Error(string message, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList()
    };
}

/// <summary>
/// Resposta de API sem dados
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };
    
    public static ApiResponse Error(string message, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList()
    };
}

/// <summary>
/// Modelo de resposta de erro
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}
