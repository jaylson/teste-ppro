using System.Net;
using System.Text.Json;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Exceptions;

namespace PartnershipManager.API.Middlewares;

/// <summary>
/// Middleware para tratamento global de exceções
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
        
        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = validationEx.Message;
                errorResponse.Errors = validationEx.Errors;
                _logger.LogWarning("Erro de validação: {Message}", validationEx.Message);
                break;
                
            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundEx.Message;
                _logger.LogWarning("Recurso não encontrado: {Entity} - {Key}", notFoundEx.EntityName, notFoundEx.Key);
                break;
                
            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorizedEx.Message ?? ErrorMessages.Unauthorized;
                _logger.LogWarning("Acesso não autorizado: {Path}", context.Request.Path);
                break;
                
            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = ErrorMessages.Forbidden;
                _logger.LogWarning("Acesso proibido: {Path}", context.Request.Path);
                break;
                
            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = conflictEx.Message;
                _logger.LogWarning("Conflito de dados: {Message}", conflictEx.Message);
                break;
                
            case BusinessRuleException businessEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse.Message = businessEx.Message;
                _logger.LogWarning("Regra de negócio violada: {Rule}", businessEx.RuleName);
                break;
                
            case DomainException domainEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = domainEx.Message;
                _logger.LogWarning("Erro de domínio: {Message}", domainEx.Message);
                break;
                
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = ErrorMessages.InternalError;
                _logger.LogError(exception, "Erro interno não tratado");
                break;
        }
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }
}
