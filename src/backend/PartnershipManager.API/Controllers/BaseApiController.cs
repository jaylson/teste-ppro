using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Base controller with common helpers shared across all API controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }

    protected Guid GetRequiredUserId()
    {
        var id = GetUserId();
        if (!id.HasValue)
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        return id.Value;
    }
}
