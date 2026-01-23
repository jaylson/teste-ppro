# 🤖 Plano de Execução - Módulo de Usuários
## Otimizado para Agente GitHub + Claude Sonnet 4.5

**Versão:** 1.0  
**Data:** 22 de Janeiro de 2025  
**Tempo Estimado:** 12-16 horas  
**Modelo:** Claude Sonnet 4.5

---

## 📋 Instruções para o Agente

### Regras Críticas

```
⚠️ ANTES DE CRIAR QUALQUER ARQUIVO:
1. SEMPRE verifique se já existe usando: find ou grep
2. SEMPRE leia os arquivos de referência indicados
3. NUNCA duplique código - reutilize o existente
4. SIGA os padrões dos arquivos de referência exatamente
```

### Comandos de Verificação Obrigatórios

```bash
# Antes de cada tarefa, execute:
cd src/backend && dotnet build --no-restore
cd src/frontend && npm run lint && npm run build
```

---

## 🗂️ Contexto do Projeto

### Stack Tecnológica
- **Backend:** .NET 9, Dapper, MySQL 8.0, JWT
- **Frontend:** React 18, TypeScript, Vite, Tailwind CSS, Zustand, React Query

### Estrutura de Diretórios
```
src/
├── backend/
│   ├── PartnershipManager.API/Controllers/
│   ├── PartnershipManager.Application/Features/
│   ├── PartnershipManager.Domain/Entities/
│   └── PartnershipManager.Infrastructure/
└── frontend/src/
    ├── components/
    ├── hooks/
    ├── pages/
    ├── services/
    ├── stores/
    └── types/
```

---

## ✅ O QUE JÁ EXISTE (NÃO RECRIAR)

### Backend - Arquivos Existentes

| Camada | Arquivo | Status |
|--------|---------|--------|
| Domain | `Entities/User.cs` | ✅ Completo |
| Domain | `Entities/UserRole.cs` (dentro de User.cs) | ✅ Completo |
| Domain | `Enums/UserStatus.cs` | ✅ Completo |
| Domain | `Enums/Role.cs` | ✅ Completo |
| Domain | `Interfaces/Repositories.cs` | ✅ Completo |
| Domain | `Constants/Messages.cs` | ✅ Completo |
| Application | `Features/Auth/DTOs/AuthDTOs.cs` | ✅ Completo |
| Application | `Features/Auth/Validators/AuthValidators.cs` | ✅ Completo |
| Application | `Features/Users/DTOs/UserDTOs.cs` | ✅ Completo |
| Application | `Features/Users/Validators/UserValidators.cs` | ✅ Completo |
| Infrastructure | `Repositories/UserRepository.cs` | ✅ Completo |
| Infrastructure | `Services/AuthService.cs` | ✅ Completo |
| API | `Controllers/AuthController.cs` | ⚠️ Parcial |
| API | `Controllers/UsersController.cs` | ⚠️ Parcial |
| API | `Extensions/ServiceExtensions.cs` | ✅ Completo |

### Frontend - Arquivos Existentes

| Camada | Arquivo | Status |
|--------|---------|--------|
| Store | `stores/authStore.ts` | ✅ Completo |
| Pages | `pages/Login.tsx` | ⚠️ Mock (precisa conectar) |
| Layout | `components/layout/AuthLayout.tsx` | ✅ Completo |
| Layout | `components/layout/MainLayout.tsx` | ✅ Completo |
| Constants | `constants/index.ts` | ✅ Completo |
| App | `App.tsx` | ✅ Completo |

---

## 🎯 TAREFAS A EXECUTAR

### Ordem de Execução Otimizada

```
FASE 1: Backend (endpoints faltantes)
    └── Task 1.1 → Task 1.2 → Task 1.3 → Task 1.4

FASE 2: Frontend Services
    └── Task 2.1 → Task 2.2 → Task 2.3

FASE 3: Frontend Pages
    └── Task 3.1 → Task 3.2 → Task 3.3 → Task 3.4

FASE 4: Integração
    └── Task 4.1 → Task 4.2
```

---

## FASE 1: Backend - Completar Endpoints

### Task 1.1: Implementar Refresh Token

**Arquivo:** `src/backend/PartnershipManager.API/Controllers/AuthController.cs`

**Ação:** ADICIONAR endpoint ao controller existente

**Referência:** Ler o arquivo existente primeiro

```csharp
// ADICIONAR este endpoint ao AuthController.cs existente

/// <summary>
/// Renova o token de acesso usando refresh token
/// </summary>
[HttpPost("refresh")]
[AllowAnonymous]
[ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    if (string.IsNullOrEmpty(request.RefreshToken))
    {
        throw new UnauthorizedException(ErrorMessages.InvalidRefreshToken);
    }
    
    // Buscar usuário pelo refresh token
    var users = await _unitOfWork.Users.GetAllAsync();
    var user = users.FirstOrDefault(u => 
        u.RefreshToken == request.RefreshToken && 
        u.RefreshTokenExpiry > DateTime.UtcNow &&
        !u.IsDeleted);
    
    if (user == null)
    {
        throw new UnauthorizedException(ErrorMessages.InvalidRefreshToken);
    }
    
    // Gerar novos tokens
    var roles = await _unitOfWork.UserRoles.GetRoleNamesByUserIdAsync(user.Id);
    var newAccessToken = _authService.GenerateJwtToken(user, roles);
    var newRefreshToken = _authService.GenerateRefreshToken();
    
    // Atualizar refresh token no banco
    var expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    await _unitOfWork.Users.UpdateRefreshTokenAsync(
        user.Id, 
        newRefreshToken, 
        DateTime.UtcNow.AddDays(expirationDays));
    
    var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId);
    
    var response = new AuthResponse
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken,
        ExpiresAt = DateTime.UtcNow.AddHours(
            int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
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
    
    return Ok(ApiResponse<AuthResponse>.Ok(response));
}
```

**Verificação:**
```bash
cd src/backend && dotnet build
# Testar via Swagger: POST /api/auth/refresh
```

---

### Task 1.2: Implementar Change Password

**Arquivo:** `src/backend/PartnershipManager.API/Controllers/AuthController.cs`

**Ação:** ADICIONAR endpoint ao controller existente

```csharp
// ADICIONAR este endpoint ao AuthController.cs existente

/// <summary>
/// Altera a senha do usuário autenticado
/// </summary>
[HttpPost("change-password")]
[Authorize]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    
    // Verificar senha atual
    if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
    {
        throw new ValidationException("Senha atual incorreta");
    }
    
    // Verificar se nova senha é igual à atual
    if (request.CurrentPassword == request.NewPassword)
    {
        throw new ValidationException(ErrorMessages.PasswordSameAsCurrent);
    }
    
    // Atualizar senha
    var newPasswordHash = _authService.HashPassword(request.NewPassword);
    user.UpdatePassword(newPasswordHash);
    await _unitOfWork.Users.UpdateAsync(user);
    
    // Invalidar refresh token (força novo login)
    await _unitOfWork.Users.UpdateRefreshTokenAsync(user.Id, null, null);
    
    _logger.LogInformation("Senha alterada para usuário: {UserId}", user.Id);
    
    return Ok(ApiResponse.Ok(SuccessMessages.PasswordChanged));
}
```

**Adicionar método na Entity User (se não existir):**

Verificar em `src/backend/PartnershipManager.Domain/Entities/User.cs`:

```csharp
// Se não existir, ADICIONAR este método à classe User
public void UpdatePassword(string newPasswordHash)
{
    PasswordHash = newPasswordHash;
    UpdatedAt = DateTime.UtcNow;
    // Resetar tentativas de login
    FailedLoginAttempts = 0;
    LockoutEnd = null;
}
```

**Adicionar constante em Messages.cs (se não existir):**

Verificar em `src/backend/PartnershipManager.Domain/Constants/Messages.cs`:

```csharp
// Se não existir em SuccessMessages, ADICIONAR:
public const string PasswordChanged = "Senha alterada com sucesso.";

// Se não existir em ErrorMessages, ADICIONAR:
public const string PasswordSameAsCurrent = "A nova senha não pode ser igual à senha atual.";
public const string InvalidRefreshToken = "Refresh token inválido ou expirado.";
```

---

### Task 1.3: Implementar Endpoints de Roles

**Arquivo:** `src/backend/PartnershipManager.API/Controllers/UsersController.cs`

**Ação:** ADICIONAR endpoints ao controller existente

```csharp
// ADICIONAR estes endpoints ao UsersController.cs existente

/// <summary>
/// Adiciona um papel ao usuário
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
    
    // Verificar se pertence à mesma empresa
    if (user.CompanyId != _currentUserService.CompanyId)
    {
        throw new ForbiddenException();
    }
    
    // Verificar se já tem o papel
    if (await _unitOfWork.UserRoles.ExistsAsync(id, request.Role.ToString()))
    {
        throw new ConflictException("Usuário já possui este papel.");
    }
    
    var userRole = UserRole.Create(id, request.Role, _currentUserService.UserId);
    if (request.ExpiresAt.HasValue)
    {
        userRole.ExpiresAt = request.ExpiresAt;
    }
    
    await _unitOfWork.UserRoles.AddAsync(userRole);
    
    // Invalidar cache
    await _cacheService.RemoveAsync(CacheKeys.UserRoles(id));
    
    _logger.LogInformation("Papel {Role} adicionado ao usuário {UserId}", request.Role, id);
    
    return Ok(ApiResponse.Ok(SuccessMessages.RoleAdded));
}

/// <summary>
/// Remove um papel do usuário
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
    
    // Verificar se pertence à mesma empresa
    if (user.CompanyId != _currentUserService.CompanyId)
    {
        throw new ForbiddenException();
    }
    
    // Verificar se tem o papel
    if (!await _unitOfWork.UserRoles.ExistsAsync(id, role))
    {
        throw new NotFoundException("Papel", role);
    }
    
    await _unitOfWork.UserRoles.DeactivateAsync(id, role);
    
    // Invalidar cache
    await _cacheService.RemoveAsync(CacheKeys.UserRoles(id));
    
    _logger.LogInformation("Papel {Role} removido do usuário {UserId}", role, id);
    
    return Ok(ApiResponse.Ok(SuccessMessages.RoleRemoved));
}

/// <summary>
/// Ativa um usuário desativado
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
    
    if (user.CompanyId != _currentUserService.CompanyId)
    {
        throw new ForbiddenException();
    }
    
    user.Activate();
    await _unitOfWork.Users.UpdateAsync(user);
    
    await _cacheService.RemoveAsync(CacheKeys.User(id));
    
    _logger.LogInformation("Usuário ativado: {UserId}", id);
    
    return Ok(ApiResponse.Ok(SuccessMessages.UserActivated));
}
```

**Adicionar método na Entity User (se não existir):**

```csharp
// Se não existir, ADICIONAR à classe User em User.cs
public void Activate()
{
    Status = UserStatus.Active;
    UpdatedAt = DateTime.UtcNow;
}

public void Deactivate()
{
    Status = UserStatus.Inactive;
    UpdatedAt = DateTime.UtcNow;
}
```

---

### Task 1.4: Implementar UserRoleRepository

**Verificar primeiro:** `src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/`

**Se NÃO existir** `UserRoleRepository.cs`, criar:

**Arquivo:** `src/backend/PartnershipManager.Infrastructure/Persistence/Repositories/UserRoleRepository.cs`

```csharp
using System.Data;
using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de papéis de usuário
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly DapperContext _context;
    
    private IDbConnection Connection => _context.Connection;
    private IDbTransaction? Transaction => _context.Transaction;

    private const string SelectColumns = @"
        id AS Id,
        user_id AS UserId,
        role AS Role,
        permissions AS Permissions,
        granted_by AS GrantedBy,
        granted_at AS GrantedAt,
        expires_at AS ExpiresAt,
        is_active AS IsActive,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt";

    public UserRoleRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        var sql = $@"SELECT {SelectColumns} 
                     FROM user_roles 
                     WHERE user_id = @UserId 
                       AND is_active = 1 
                       AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        return await Connection.QueryAsync<UserRole>(sql, 
            new { UserId = userId.ToString() }, Transaction);
    }

    public async Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId)
    {
        var sql = @"SELECT role 
                    FROM user_roles 
                    WHERE user_id = @UserId 
                      AND is_active = 1 
                      AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        return await Connection.QueryAsync<string>(sql, 
            new { UserId = userId.ToString() }, Transaction);
    }

    public async Task AddAsync(UserRole userRole)
    {
        var sql = @"INSERT INTO user_roles 
                    (id, user_id, role, permissions, granted_by, granted_at, expires_at, is_active, created_at, updated_at)
                    VALUES 
                    (@Id, @UserId, @Role, @Permissions, @GrantedBy, @GrantedAt, @ExpiresAt, @IsActive, @CreatedAt, @UpdatedAt)";
        
        await Connection.ExecuteAsync(sql, new
        {
            Id = userRole.Id.ToString(),
            UserId = userRole.UserId.ToString(),
            Role = userRole.Role.ToString(),
            userRole.Permissions,
            GrantedBy = userRole.GrantedBy?.ToString(),
            userRole.GrantedAt,
            userRole.ExpiresAt,
            userRole.IsActive,
            userRole.CreatedAt,
            userRole.UpdatedAt
        }, Transaction);
    }

    public async Task DeactivateAsync(Guid userId, string role)
    {
        var sql = @"UPDATE user_roles 
                    SET is_active = 0, updated_at = UTC_TIMESTAMP() 
                    WHERE user_id = @UserId AND role = @Role AND is_active = 1";
        
        await Connection.ExecuteAsync(sql, 
            new { UserId = userId.ToString(), Role = role }, Transaction);
    }

    public async Task<bool> ExistsAsync(Guid userId, string role)
    {
        var sql = @"SELECT COUNT(1) FROM user_roles 
                    WHERE user_id = @UserId 
                      AND role = @Role 
                      AND is_active = 1 
                      AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP())";
        
        var count = await Connection.ExecuteScalarAsync<int>(sql, 
            new { UserId = userId.ToString(), Role = role }, Transaction);
        
        return count > 0;
    }
}
```

**Verificar registro DI em ServiceExtensions.cs:**

```csharp
// Se não estiver registrado, adicionar em AddInfrastructureServices:
services.AddScoped<IUserRoleRepository, UserRoleRepository>();
```

---

## FASE 2: Frontend - Services e Types

### Task 2.1: Criar Types

**Arquivo:** `src/frontend/src/types/auth.types.ts`

**Verificar se existe primeiro. Se não existir, criar:**

```typescript
// Types para autenticação - espelham os DTOs do backend

export interface LoginRequest {
  email: string;
  password: string;
  companyId: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  companyId: string;
  companyName: string;
  roles: string[];
  language: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
  companyId: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}
```

**Arquivo:** `src/frontend/src/types/user.types.ts`

```typescript
// Types para usuários - espelham os DTOs do backend

export interface User {
  id: string;
  companyId: string;
  email: string;
  name: string;
  avatarUrl?: string;
  phone?: string;
  status: UserStatus;
  language: Language;
  timezone: string;
  twoFactorEnabled: boolean;
  lastLoginAt?: string;
  roles: string[];
  createdAt: string;
  updatedAt: string;
}

export interface UserSummary {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  status: string;
  roles: string[];
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  phone?: string;
  initialRole: Role;
}

export interface UpdateUserRequest {
  name: string;
  phone?: string;
  avatarUrl?: string;
}

export interface ManageUserRoleRequest {
  role: Role;
  expiresAt?: string;
}

export type UserStatus = 'Active' | 'Inactive' | 'Pending';
export type Language = 'PT' | 'EN' | 'ES';
export type Role = 
  | 'SuperAdmin' 
  | 'Admin' 
  | 'Founder' 
  | 'BoardMember' 
  | 'Legal' 
  | 'Finance' 
  | 'HR' 
  | 'Employee' 
  | 'Investor' 
  | 'Viewer';
```

**Arquivo:** `src/frontend/src/types/index.ts`

```typescript
// Re-export all types
export * from './auth.types';
export * from './user.types';

// Common types
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  search?: string;
}
```

---

### Task 2.2: Criar Auth Service

**Arquivo:** `src/frontend/src/services/authService.ts`

**Referência:** Verificar `src/frontend/src/services/api.ts` para padrão de chamadas

```typescript
import { api } from './api';
import type { 
  LoginRequest, 
  AuthResponse, 
  RefreshTokenRequest,
  ChangePasswordRequest,
  UserInfo 
} from '@/types';

const AUTH_ENDPOINTS = {
  LOGIN: '/auth/login',
  LOGOUT: '/auth/logout',
  REFRESH: '/auth/refresh',
  ME: '/auth/me',
  CHANGE_PASSWORD: '/auth/change-password',
};

export const authService = {
  /**
   * Realiza login no sistema
   */
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<{ data: AuthResponse }>(AUTH_ENDPOINTS.LOGIN, data);
    return response.data.data;
  },

  /**
   * Realiza logout
   */
  logout: async (): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.LOGOUT);
  },

  /**
   * Renova o token de acesso
   */
  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await api.post<{ data: AuthResponse }>(AUTH_ENDPOINTS.REFRESH, {
      refreshToken,
    } as RefreshTokenRequest);
    return response.data.data;
  },

  /**
   * Obtém dados do usuário autenticado
   */
  getMe: async (): Promise<UserInfo> => {
    const response = await api.get<{ data: UserInfo }>(AUTH_ENDPOINTS.ME);
    return response.data.data;
  },

  /**
   * Altera a senha do usuário
   */
  changePassword: async (data: ChangePasswordRequest): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.CHANGE_PASSWORD, data);
  },
};
```

---

### Task 2.3: Criar User Service

**Arquivo:** `src/frontend/src/services/userService.ts`

```typescript
import { api } from './api';
import type { 
  User, 
  UserSummary, 
  CreateUserRequest, 
  UpdateUserRequest,
  ManageUserRoleRequest,
  PagedResult,
  PaginationParams 
} from '@/types';

const USERS_ENDPOINT = '/users';

export const userService = {
  /**
   * Lista usuários com paginação
   */
  getAll: async (params?: PaginationParams): Promise<PagedResult<UserSummary>> => {
    const response = await api.get<{ data: PagedResult<UserSummary> }>(USERS_ENDPOINT, { params });
    return response.data.data;
  },

  /**
   * Obtém usuário por ID
   */
  getById: async (id: string): Promise<User> => {
    const response = await api.get<{ data: User }>(`${USERS_ENDPOINT}/${id}`);
    return response.data.data;
  },

  /**
   * Cria novo usuário
   */
  create: async (data: CreateUserRequest): Promise<User> => {
    const response = await api.post<{ data: User }>(USERS_ENDPOINT, data);
    return response.data.data;
  },

  /**
   * Atualiza usuário
   */
  update: async (id: string, data: UpdateUserRequest): Promise<User> => {
    const response = await api.put<{ data: User }>(`${USERS_ENDPOINT}/${id}`, data);
    return response.data.data;
  },

  /**
   * Remove usuário (soft delete)
   */
  delete: async (id: string): Promise<void> => {
    await api.delete(`${USERS_ENDPOINT}/${id}`);
  },

  /**
   * Adiciona papel ao usuário
   */
  addRole: async (userId: string, data: ManageUserRoleRequest): Promise<void> => {
    await api.post(`${USERS_ENDPOINT}/${userId}/roles`, data);
  },

  /**
   * Remove papel do usuário
   */
  removeRole: async (userId: string, role: string): Promise<void> => {
    await api.delete(`${USERS_ENDPOINT}/${userId}/roles/${role}`);
  },

  /**
   * Ativa usuário
   */
  activate: async (id: string): Promise<void> => {
    await api.post(`${USERS_ENDPOINT}/${id}/activate`);
  },
};
```

---

### Task 2.4: Configurar Interceptor de Refresh Token

**Arquivo:** `src/frontend/src/services/api.ts`

**Ação:** MODIFICAR o arquivo existente para adicionar interceptor

**Verificar estrutura atual primeiro, então adicionar:**

```typescript
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '@/stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Flag para evitar múltiplos refreshes simultâneos
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: Error) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

// Interceptor de request - adiciona token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor de response - refresh automático
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Se não for 401 ou já tentou retry, rejeita
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // Se já está fazendo refresh, aguarda na fila
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const { refreshToken, setAuth, logout } = useAuthStore.getState();

    if (!refreshToken) {
      logout();
      return Promise.reject(error);
    }

    try {
      const response = await axios.post(`${API_URL}/auth/refresh`, {
        refreshToken,
      });

      const { accessToken: newAccessToken, refreshToken: newRefreshToken, user } = response.data.data;

      setAuth(user, newAccessToken, newRefreshToken);

      processQueue(null, newAccessToken);

      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError as Error, null);
      logout();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

export default api;
```

---

## FASE 3: Frontend - Hooks e Pages

### Task 3.1: Criar Hook useUsers

**Arquivo:** `src/frontend/src/hooks/useUsers.ts`

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService } from '@/services/userService';
import type { 
  CreateUserRequest, 
  UpdateUserRequest, 
  ManageUserRoleRequest,
  PaginationParams 
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['users'];

export function useUsers(params?: PaginationParams) {
  return useQuery({
    queryKey: [...QUERY_KEY, params],
    queryFn: () => userService.getAll(params),
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => userService.getById(id),
    enabled: !!id,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateUserRequest) => userService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Usuário criado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar usuário');
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserRequest }) =>
      userService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Usuário atualizado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar usuário');
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => userService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Usuário removido com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover usuário');
    },
  });
}

export function useAddUserRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, data }: { userId: string; data: ManageUserRoleRequest }) =>
      userService.addRole(userId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Papel adicionado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao adicionar papel');
    },
  });
}

export function useRemoveUserRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) =>
      userService.removeRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Papel removido com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover papel');
    },
  });
}

export function useActivateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => userService.activate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Usuário ativado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao ativar usuário');
    },
  });
}
```

---

### Task 3.2: Atualizar Login.tsx para usar API real

**Arquivo:** `src/frontend/src/pages/Login.tsx`

**Ação:** MODIFICAR o arquivo existente

**Substituir a função onSubmit mockada pela real:**

```typescript
// LOCALIZAR e SUBSTITUIR a função onSubmit existente por:

const onSubmit = async (data: LoginForm) => {
  setLoading(true);

  try {
    // CompanyId fixo para demo - em produção, seria selecionado
    const companyId = import.meta.env.VITE_DEFAULT_COMPANY_ID || 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';
    
    const response = await authService.login({
      email: data.email,
      password: data.password,
      companyId,
    });

    setAuth(
      response.user,
      response.accessToken,
      response.refreshToken
    );
    
    toast.success('Login realizado com sucesso!');
    navigate('/dashboard');
  } catch (error: any) {
    const message = error.response?.data?.message || 'Credenciais inválidas';
    toast.error(message);
  } finally {
    setLoading(false);
  }
};
```

**Adicionar import no topo do arquivo:**

```typescript
import { authService } from '@/services/authService';
```

---

### Task 3.3: Criar Página de Listagem de Usuários

**Arquivo:** `src/frontend/src/pages/settings/Users.tsx`

```typescript
import { useState } from 'react';
import { Plus, Search, MoreVertical, Edit, Trash2, Shield } from 'lucide-react';
import { Button, Input, Badge, Card } from '@/components/ui';
import { useUsers, useDeleteUser, useActivateUser } from '@/hooks/useUsers';
import { UserForm } from '@/components/users/UserForm';
import type { UserSummary } from '@/types';
import toast from 'react-hot-toast';

export default function Users() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingUser, setEditingUser] = useState<UserSummary | null>(null);

  const { data, isLoading, error } = useUsers({ page, pageSize: 10, search });
  const deleteUser = useDeleteUser();
  const activateUser = useActivateUser();

  const handleDelete = async (user: UserSummary) => {
    if (confirm(`Deseja realmente desativar o usuário ${user.name}?`)) {
      deleteUser.mutate(user.id);
    }
  };

  const handleActivate = async (user: UserSummary) => {
    activateUser.mutate(user.id);
  };

  const getStatusBadge = (status: string) => {
    const variants: Record<string, 'success' | 'warning' | 'default'> = {
      Active: 'success',
      Pending: 'warning',
      Inactive: 'default',
    };
    return <Badge variant={variants[status] || 'default'}>{status}</Badge>;
  };

  if (error) {
    return (
      <div className="p-6">
        <Card className="p-8 text-center">
          <p className="text-red-500">Erro ao carregar usuários</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-primary">Usuários</h1>
          <p className="text-primary-500">Gerencie os usuários da sua empresa</p>
        </div>
        <Button onClick={() => setShowForm(true)} icon={<Plus className="w-4 h-4" />}>
          Novo Usuário
        </Button>
      </div>

      {/* Search */}
      <Card className="p-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
          <Input
            placeholder="Buscar por nome ou email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-10"
          />
        </div>
      </Card>

      {/* Table */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-primary-50">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">Nome</th>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">Email</th>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">Papéis</th>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">Status</th>
                <th className="px-4 py-3 text-right text-sm font-medium text-primary-600">Ações</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-primary-100">
              {isLoading ? (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-primary-500">
                    Carregando...
                  </td>
                </tr>
              ) : data?.items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-primary-500">
                    Nenhum usuário encontrado
                  </td>
                </tr>
              ) : (
                data?.items.map((user) => (
                  <tr key={user.id} className="hover:bg-primary-50">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-primary-200 flex items-center justify-center">
                          <span className="text-sm font-medium text-primary-700">
                            {user.name.charAt(0).toUpperCase()}
                          </span>
                        </div>
                        <span className="font-medium text-primary">{user.name}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-primary-600">{user.email}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1">
                        {user.roles.map((role) => (
                          <Badge key={role} variant="info" size="sm">
                            {role}
                          </Badge>
                        ))}
                      </div>
                    </td>
                    <td className="px-4 py-3">{getStatusBadge(user.status)}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setEditingUser(user)}
                          className="p-1 hover:bg-primary-100 rounded"
                          title="Editar"
                        >
                          <Edit className="w-4 h-4 text-primary-500" />
                        </button>
                        {user.status === 'Inactive' ? (
                          <button
                            onClick={() => handleActivate(user)}
                            className="p-1 hover:bg-green-100 rounded"
                            title="Ativar"
                          >
                            <Shield className="w-4 h-4 text-green-500" />
                          </button>
                        ) : (
                          <button
                            onClick={() => handleDelete(user)}
                            className="p-1 hover:bg-red-100 rounded"
                            title="Desativar"
                          >
                            <Trash2 className="w-4 h-4 text-red-500" />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="px-4 py-3 border-t border-primary-100 flex items-center justify-between">
            <p className="text-sm text-primary-500">
              Mostrando {data.items.length} de {data.total} usuários
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page === 1}
                onClick={() => setPage(page - 1)}
              >
                Anterior
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page === data.totalPages}
                onClick={() => setPage(page + 1)}
              >
                Próximo
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Modal Form */}
      {(showForm || editingUser) && (
        <UserForm
          user={editingUser}
          onClose={() => {
            setShowForm(false);
            setEditingUser(null);
          }}
        />
      )}
    </div>
  );
}
```

---

### Task 3.4: Criar Componente UserForm

**Arquivo:** `src/frontend/src/components/users/UserForm.tsx`

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { Button, Input, Select } from '@/components/ui';
import { useCreateUser, useUpdateUser } from '@/hooks/useUsers';
import type { UserSummary, Role } from '@/types';
import { Roles, RoleLabels } from '@/constants';

const createUserSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(200),
  email: z.string().email('Email inválido').min(1, 'Email é obrigatório'),
  password: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Deve conter letra maiúscula')
    .regex(/[a-z]/, 'Deve conter letra minúscula')
    .regex(/[0-9]/, 'Deve conter número')
    .regex(/[^a-zA-Z0-9]/, 'Deve conter caractere especial'),
  phone: z.string().optional(),
  initialRole: z.string().min(1, 'Papel é obrigatório'),
});

const updateUserSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(200),
  phone: z.string().optional(),
  avatarUrl: z.string().url('URL inválida').optional().or(z.literal('')),
});

type CreateUserForm = z.infer<typeof createUserSchema>;
type UpdateUserForm = z.infer<typeof updateUserSchema>;

interface UserFormProps {
  user?: UserSummary | null;
  onClose: () => void;
}

export function UserForm({ user, onClose }: UserFormProps) {
  const isEditing = !!user;
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserForm | UpdateUserForm>({
    resolver: zodResolver(isEditing ? updateUserSchema : createUserSchema),
    defaultValues: isEditing
      ? { name: user.name }
      : { initialRole: 'Viewer' },
  });

  const onSubmit = async (data: CreateUserForm | UpdateUserForm) => {
    try {
      if (isEditing) {
        await updateUser.mutateAsync({
          id: user.id,
          data: data as UpdateUserForm,
        });
      } else {
        await createUser.mutateAsync(data as CreateUserForm);
      }
      onClose();
    } catch (error) {
      // Error handled by mutation
    }
  };

  const roleOptions = Object.entries(RoleLabels).map(([value, label]) => ({
    value,
    label,
  }));

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md mx-4">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-lg font-semibold text-primary">
            {isEditing ? 'Editar Usuário' : 'Novo Usuário'}
          </h2>
          <button
            onClick={onClose}
            className="p-1 hover:bg-primary-100 rounded"
          >
            <X className="w-5 h-5 text-primary-500" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-4 space-y-4">
          <Input
            label="Nome"
            placeholder="Nome completo"
            error={errors.name?.message}
            {...register('name')}
          />

          {!isEditing && (
            <>
              <Input
                label="Email"
                type="email"
                placeholder="email@exemplo.com"
                error={(errors as any).email?.message}
                {...register('email')}
              />

              <Input
                label="Senha"
                type="password"
                placeholder="••••••••"
                error={(errors as any).password?.message}
                {...register('password')}
              />

              <Select
                label="Papel Inicial"
                options={roleOptions}
                error={(errors as any).initialRole?.message}
                {...register('initialRole')}
              />
            </>
          )}

          <Input
            label="Telefone"
            placeholder="(00) 00000-0000"
            error={errors.phone?.message}
            {...register('phone')}
          />

          {isEditing && (
            <Input
              label="URL do Avatar"
              placeholder="https://..."
              error={(errors as any).avatarUrl?.message}
              {...register('avatarUrl')}
            />
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              className="flex-1"
              onClick={onClose}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              className="flex-1"
              loading={isSubmitting}
            >
              {isEditing ? 'Salvar' : 'Criar'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
```

---

## FASE 4: Integração e Rotas

### Task 4.1: Adicionar Rota de Usuários

**Arquivo:** `src/frontend/src/App.tsx`

**Ação:** MODIFICAR para adicionar rota

```typescript
// ADICIONAR import no topo:
import Users from './pages/settings/Users';

// ADICIONAR rota dentro das Protected Routes:
<Route path="/settings/users" element={<Users />} />
```

---

### Task 4.2: Adicionar Link no Menu

**Arquivo:** `src/frontend/src/components/layout/Sidebar.tsx`

**Ação:** Verificar e adicionar item de menu se não existir

```typescript
// Adicionar item de menu para Usuários em Settings
{
  name: 'Usuários',
  path: '/settings/users',
  icon: Users, // import { Users } from 'lucide-react'
}
```

---

## 📋 Checklist de Verificação Final

Execute após todas as tarefas:

### Backend
```bash
cd src/backend

# Build
dotnet build --no-restore

# Executar (verificar se sobe sem erros)
dotnet run --project PartnershipManager.API

# Testar endpoints via Swagger (http://localhost:5000/swagger):
# - POST /api/auth/login
# - POST /api/auth/refresh
# - POST /api/auth/change-password
# - GET /api/users
# - POST /api/users
# - POST /api/users/{id}/roles
# - DELETE /api/users/{id}/roles/{role}
```

### Frontend
```bash
cd src/frontend

# Lint
npm run lint

# Build
npm run build

# Executar
npm run dev

# Testar manualmente:
# - Login com credenciais válidas
# - Navegação para /settings/users
# - Criar novo usuário
# - Editar usuário
# - Adicionar/remover papel
```

---

## 🚨 Troubleshooting

### Erro: "Cannot find module"
```bash
# Frontend
npm install

# Backend
dotnet restore
```

### Erro: "401 Unauthorized"
- Verificar se token está sendo enviado no header
- Verificar validade do token (expiration)
- Verificar se refresh está funcionando

### Erro: "Duplicate key"
- Verificar se email já existe no banco
- Verificar se papel já foi atribuído ao usuário

### Erro: Build failed
```bash
# Limpar cache e rebuild
dotnet clean && dotnet build
npm run build -- --force
```

---

## 📊 Métricas de Sucesso

| Critério | Esperado |
|----------|----------|
| Build Backend | ✅ Sem erros |
| Build Frontend | ✅ Sem erros |
| Lint Frontend | ✅ Sem warnings |
| Login funcional | ✅ Conecta à API |
| CRUD Usuários | ✅ Todas operações |
| Refresh Token | ✅ Auto-renovação |
| Gestão de Roles | ✅ Add/Remove |

---

**Documento gerado em:** 22/01/2025  
**Para uso com:** GitHub Agent + Claude Sonnet 4.5
