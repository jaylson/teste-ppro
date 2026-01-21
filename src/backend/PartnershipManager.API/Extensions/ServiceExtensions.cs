using System.Text;
using FluentValidation;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Caching;
using PartnershipManager.Infrastructure.Jobs;
using PartnershipManager.Infrastructure.Persistence;
using PartnershipManager.Infrastructure.Persistence.Repositories;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Extensions;

/// <summary>
/// Extensões para configuração de serviços da Application
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // FluentValidation - registra todos os validators do assembly
        services.AddValidatorsFromAssemblyContaining<PartnershipManager.Application.Features.Auth.Validators.LoginValidator>();
        
        return services;
    }
}

/// <summary>
/// Extensões para configuração de serviços de infraestrutura
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - Dapper
        var connectionString = configuration.GetConnectionString("MySQL") 
            ?? throw new InvalidOperationException("MySQL connection string not found");
        
        services.AddSingleton<IDbConnectionFactory>(new MySqlConnectionFactory(connectionString));
        services.AddScoped<DapperContext>();
        
        // Repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Auth Service
        services.AddScoped<IAuthService, AuthService>();
        
        // Cache - Redis ou Memory
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "pm:";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, InMemoryCacheService>();
        }
        
        // Hangfire
        var hangfireConnection = configuration.GetConnectionString("Hangfire") ?? connectionString;
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseStorage(new MySqlStorage(
                    hangfireConnection,
                    new MySqlStorageOptions
                    {
                        TablesPrefix = "hangfire_",
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5)
                    }));
        });
        
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "default", "emails", "reports" };
        });
        
        services.AddScoped<IBackgroundJobs, BackgroundJobs>();
        
        return services;
    }
}

/// <summary>
/// Extensões para configuração de serviços da API
/// </summary>
public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
            });
        
        // CORS
        var corsOrigins = configuration["Cors:Origins"]?.Split(',') ?? new[] { "http://localhost:3000" };
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(corsOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not found");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "PartnershipManager";
        var jwtAudience = configuration["Jwt:Audience"] ?? "PartnershipManagerApp";
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddAuthorization();
        
        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Partnership Manager API",
                Version = "v1",
                Description = "API para gestão societária",
                Contact = new OpenApiContact
                {
                    Name = "Equipe Ophir",
                    Email = "contato@ophir.com.br"
                }
            });
            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Digite o token JWT"
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        // Health Checks
        services.AddHealthChecks()
            .AddMySql(
                configuration.GetConnectionString("MySQL")!,
                name: "mysql",
                tags: new[] { "db", "mysql" })
            .AddRedis(
                configuration.GetConnectionString("Redis") ?? "localhost:6379",
                name: "redis",
                tags: new[] { "cache", "redis" })
            .AddHangfire(options =>
            {
                options.MinimumAvailableServers = 1;
            }, name: "hangfire", tags: new[] { "jobs", "hangfire" });
        
        // Current User Service
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }
}

/// <summary>
/// Serviço para obter informações do usuário atual
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }
    
    public Guid? CompanyId
    {
        get
        {
            var companyId = _httpContextAccessor.HttpContext?.User
                .FindFirst("companyId")?.Value;
            return Guid.TryParse(companyId, out var id) ? id : null;
        }
    }
    
    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User
        .FindAll(System.Security.Claims.ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        .Identity?.IsAuthenticated ?? false;
}
