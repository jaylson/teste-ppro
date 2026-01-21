using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Auth.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Controller de autenticação
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Realiza login no sistema
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, request.CompanyId);
        
        if (user == null)
        {
            _logger.LogWarning("Tentativa de login com email não encontrado: {Email}", request.Email);
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }
        
        if (user.IsLockedOut)
        {
            var minutes = user.GetLockoutRemainingMinutes();
            throw new UnauthorizedException(string.Format(ErrorMessages.UserBlocked, minutes));
        }
        
        if (user.Status == UserStatus.Inactive)
        {
            throw new UnauthorizedException(ErrorMessages.UserInactive);
        }
        
        if (user.Status == UserStatus.Pending)
        {
            throw new UnauthorizedException(ErrorMessages.UserPending);
        }
        
        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _unitOfWork.Users.UpdateLoginInfoAsync(user.Id, false);
            _logger.LogWarning("Senha inválida para usuário: {Email}", request.Email);
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }
        
        // Login bem-sucedido
        await _unitOfWork.Users.UpdateLoginInfoAsync(user.Id, true);
        
        var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
        var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId);
        
        var accessToken = _authService.GenerateJwtToken(user, roles);
        var refreshToken = _authService.GenerateRefreshToken();
        
        await _unitOfWork.Users.UpdateRefreshTokenAsync(
            user.Id, 
            refreshToken, 
            DateTime.UtcNow.AddDays(SystemConstants.RefreshTokenDays));
        
        _logger.LogInformation("Login realizado com sucesso para: {Email}", request.Email);
        
        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(SystemConstants.TokenExpirationHours),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                CompanyId = user.CompanyId,
                CompanyName = company?.Name ?? string.Empty,
                Roles = roles.ToList(),
                Language = user.Language.ToString().ToLower()
            }
        };
        
        return Ok(ApiResponse<AuthResponse>.Ok(response, SuccessMessages.LoginSuccess));
    }
    
    /// <summary>
    /// Realiza logout do sistema
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userId, out var id))
        {
            await _unitOfWork.Users.UpdateRefreshTokenAsync(id, null, null);
            _logger.LogInformation("Logout realizado para usuário: {UserId}", id);
        }
        
        return Ok(ApiResponse.Ok(SuccessMessages.LogoutSuccess));
    }
    
    /// <summary>
    /// Retorna informações do usuário autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedException();
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
        var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId);
        
        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            AvatarUrl = user.AvatarUrl,
            CompanyId = user.CompanyId,
            CompanyName = company?.Name ?? string.Empty,
            Roles = roles.ToList(),
            Language = user.Language.ToString().ToLower()
        };
        
        return Ok(ApiResponse<UserInfo>.Ok(userInfo));
    }
    
    /// <summary>
    /// Altera a senha do usuário autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedException();
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new DomainException(ErrorMessages.CurrentPasswordInvalid);
        }
        
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new ValidationException("ConfirmNewPassword", ErrorMessages.PasswordMismatch);
        }
        
        user.ChangePassword(_authService.HashPassword(request.NewPassword));
        await _unitOfWork.Users.UpdateAsync(user);
        
        _logger.LogInformation("Senha alterada para usuário: {UserId}", id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.PasswordChanged));
    }
}
