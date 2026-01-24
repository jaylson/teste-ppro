using System.Security.Claims;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.API.Middlewares;

/// <summary>
/// Middleware para extrair e validar o contexto do Client a partir do token JWT
/// </summary>
public class ClientContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientContextMiddleware> _logger;
    
    public ClientContextMiddleware(RequestDelegate next, ILogger<ClientContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, ICoreClientRepository clientRepository)
    {
        // Ignorar rotas que não requerem autenticação
        if (ShouldSkipValidation(context))
        {
            await _next(context);
            return;
        }
        
        var user = context.User;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Extrair ClientId do token
            var clientIdClaim = user.FindFirst("clientId") ?? user.FindFirst("ClientId") ?? user.FindFirst("client_id");
            
            if (clientIdClaim != null && Guid.TryParse(clientIdClaim.Value, out var clientId))
            {
                // Validar se o cliente existe e está ativo
                var client = await clientRepository.GetByIdAsync(clientId);
                
                if (client == null)
                {
                    _logger.LogWarning("Cliente não encontrado no token: {ClientId}", clientId);
                    throw new UnauthorizedException("Cliente não encontrado");
                }
                
                if (client.Status.ToString() != "Active")
                {
                    _logger.LogWarning("Cliente inativo tentou acessar: {ClientId} - Status: {Status}", 
                        clientId, client.Status);
                    throw new ForbiddenException("Cliente inativo ou suspenso");
                }
                
                // Adicionar ClientId aos items do contexto HTTP
                context.Items["ClientId"] = clientId;
                context.Items["Client"] = client;
                
                _logger.LogDebug("Contexto de cliente configurado: {ClientId} - {ClientName}", 
                    clientId, client.Name);
            }
            else
            {
                _logger.LogWarning("ClientId não encontrado no token do usuário: {UserId}", 
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }
        
        await _next(context);
    }
    
    private static bool ShouldSkipValidation(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Ignorar rotas de autenticação e health check
        return path.Contains("/auth/") ||
               path.Contains("/health") ||
               path.Contains("/swagger") ||
               path.Contains("/_framework") ||
               path.Contains("/api-docs");
    }
}

/// <summary>
/// Extensão para facilitar o uso do ClientContextMiddleware
/// </summary>
public static class ClientContextMiddlewareExtensions
{
    public static IApplicationBuilder UseClientContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ClientContextMiddleware>();
    }
}

/// <summary>
/// Extensões para acessar o contexto do Client
/// </summary>
public static class HttpContextClientExtensions
{
    public static Guid? GetClientId(this HttpContext context)
    {
        return context.Items["ClientId"] as Guid?;
    }
    
    public static Guid GetRequiredClientId(this HttpContext context)
    {
        var clientId = context.GetClientId();
        if (!clientId.HasValue)
        {
            throw new UnauthorizedException("ClientId não encontrado no contexto");
        }
        return clientId.Value;
    }
}
