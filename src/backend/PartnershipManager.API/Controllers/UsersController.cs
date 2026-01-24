using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Users.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Controller de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        ILogger<UsersController> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    /// <summary>
    /// Lista usuários da empresa com paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var companyId = _currentUserService.CompanyId 
            ?? throw new UnauthorizedException();
        
        pageSize = Math.Min(pageSize, SystemConstants.MaxPageSize);
        
        var (users, total) = await _unitOfWork.Users.GetPagedByCompanyAsync(
            companyId, page, pageSize, search);
        
        var items = new List<UserSummaryResponse>();
        foreach (var user in users)
        {
            var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
            items.Add(MapToSummary(user, roles));
        }
        
        var pagedResult = new PagedResult<UserSummaryResponse>(items, total, page, pageSize);
        
        return Ok(ApiResponse<PagedResult<UserSummaryResponse>>.Ok(pagedResult));
    }
    
    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Verificar se pertence à mesma empresa
        if (user.CompanyId != _currentUserService.CompanyId)
        {
            throw new ForbiddenException();
        }
        
        var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
        
        return Ok(ApiResponse<UserResponse>.Ok(MapToResponse(user, roles)));
    }
    
    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,HR")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var companyId = _currentUserService.CompanyId 
            ?? throw new UnauthorizedException();
        
        // Verificar se email já existe
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, companyId))
        {
            throw new ConflictException(ErrorMessages.UserAlreadyExists);
        }
        
        var passwordHash = _authService.HashPassword(request.Password);
        
        var user = Domain.Entities.User.Create(
            companyId,
            request.Email,
            request.Name,
            passwordHash);
        
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            await _unitOfWork.Users.AddAsync(user);
            
            // Adicionar papel inicial
            var userRole = UserRole.Create(user.Id, request.InitialRole, _currentUserService.UserId);
            await _unitOfWork.UserRoles.AddAsync(userRole);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("Usuário criado: {UserId} - {Email}", user.Id, user.Email);
            
            var roles = new[] { request.InitialRole.ToString() };
            var response = MapToResponse(user, roles);
            
            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                ApiResponse<UserResponse>.Ok(response, SuccessMessages.UserCreated));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    
    /// <summary>
    /// Atualiza um usuário
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Verificar permissão (próprio usuário ou admin)
        if (user.Id != _currentUserService.UserId && 
            !_currentUserService.Roles.Any(r => r == "Admin" || r == "SuperAdmin" || r == "HR"))
        {
            throw new ForbiddenException();
        }
        
        user.UpdateProfile(request.Name, request.Phone, request.AvatarUrl);
        await _unitOfWork.Users.UpdateAsync(user);
        
        await _cacheService.RemoveAsync(CacheKeys.User(id));
        
        _logger.LogInformation("Usuário atualizado: {UserId}", id);
        
        var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
        
        return Ok(ApiResponse<UserResponse>.Ok(MapToResponse(user, roles), SuccessMessages.UserUpdated));
    }
    
    /// <summary>
    /// Atualiza preferências do usuário
    /// </summary>
    [HttpPatch("{id:guid}/preferences")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] UpdateUserPreferencesRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Apenas o próprio usuário pode alterar preferências
        if (user.Id != _currentUserService.UserId)
        {
            throw new ForbiddenException();
        }
        
        user.UpdatePreferences(request.Language, request.Timezone, null);
        await _unitOfWork.Users.UpdateAsync(user);
        
        await _cacheService.RemoveAsync(CacheKeys.User(id));
        
        return Ok(ApiResponse.Ok(SuccessMessages.UserUpdated));
    }
    
    /// <summary>
    /// Adiciona papel ao usuário
    /// </summary>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddRole(Guid id, [FromBody] ManageUserRoleRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Verificar se já tem o papel
        if (await _unitOfWork.UserRoles.ExistsAsync(id, request.Role.ToString()))
        {
            throw new ConflictException(ErrorMessages.UserAlreadyHasRole);
        }
        
        var userRole = UserRole.Create(id, request.Role, _currentUserService.UserId);
        if (request.ExpiresAt.HasValue)
        {
            // Definir expiração no UserRole - seria necessário adicionar método
        }
        
        await _unitOfWork.UserRoles.AddAsync(userRole);
        
        await _cacheService.RemoveAsync(CacheKeys.UserRoles(id));
        
        _logger.LogInformation("Papel {Role} adicionado ao usuário {UserId}", request.Role, id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.RoleAdded));
    }
    
    /// <summary>
    /// Remove papel do usuário
    /// </summary>
    [HttpDelete("{id:guid}/roles/{role}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid id, string role)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Verificar se tem o papel
        if (!await _unitOfWork.UserRoles.ExistsAsync(id, role))
        {
            throw new NotFoundException("Papel", role);
        }
        
        // Não permitir remover o último admin
        if (role == "Admin" && user.CompanyId.HasValue)
        {
            var admins = await _unitOfWork.Users.GetActiveUsersByCompanyAsync(user.CompanyId.Value);
            var adminCount = 0;
            foreach (var admin in admins)
            {
                if (await _unitOfWork.UserRoles.ExistsAsync(admin.Id, "Admin"))
                    adminCount++;
            }
            
            if (adminCount <= 1)
            {
                throw new BusinessRuleException("LastAdmin", ErrorMessages.CannotRemoveLastAdmin);
            }
        }
        
        await _unitOfWork.UserRoles.DeactivateAsync(id, role);
        
        await _cacheService.RemoveAsync(CacheKeys.UserRoles(id));
        
        _logger.LogInformation("Papel {Role} removido do usuário {UserId}", role, id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.RoleRemoved));
    }
    
    /// <summary>
    /// Ativa um usuário
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin,HR")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        user.Activate();
        await _unitOfWork.Users.UpdateAsync(user);
        
        await _cacheService.RemoveAsync(CacheKeys.User(id));
        
        _logger.LogInformation("Usuário ativado: {UserId}", id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.UserActivated));
    }
    
    /// <summary>
    /// Desativa um usuário
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin,HR")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Não pode desativar a si mesmo
        if (user.Id == _currentUserService.UserId)
        {
            throw new BusinessRuleException("SelfDeactivation", ErrorMessages.CannotDeactivateYourself);
        }
        
        user.Deactivate();
        await _unitOfWork.Users.UpdateAsync(user);
        
        await _cacheService.RemoveAsync(CacheKeys.User(id));
        
        _logger.LogInformation("Usuário desativado: {UserId}", id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.UserDeactivated));
    }
    
    /// <summary>
    /// Exclui um usuário (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _unitOfWork.Users.ExistsAsync(id);
        
        if (!exists)
        {
            throw new NotFoundException("Usuário", id);
        }
        
        // Não pode excluir a si mesmo
        if (id == _currentUserService.UserId)
        {
            throw new BusinessRuleException("SelfDeletion", "Não é possível excluir sua própria conta.");
        }
        
        await _unitOfWork.Users.SoftDeleteAsync(id, _currentUserService.UserId);
        
        await _cacheService.RemoveAsync(CacheKeys.User(id));
        
        _logger.LogInformation("Usuário excluído: {UserId}", id);
        
        return Ok(ApiResponse.Ok(SuccessMessages.UserDeleted));
    }
    
    private static UserResponse MapToResponse(User user, IEnumerable<string> roles)
    {
        return new UserResponse
        {
            Id = user.Id,
            CompanyId = user.CompanyId ?? Guid.Empty,
            Email = user.Email,
            Name = user.Name,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            Status = user.Status.ToString(),
            Language = user.Language.ToString(),
            Timezone = user.Timezone,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
    
    private static UserSummaryResponse MapToSummary(User user, IEnumerable<string> roles)
    {
        return new UserSummaryResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status.ToString(),
            Roles = roles.ToList()
        };
    }
}
