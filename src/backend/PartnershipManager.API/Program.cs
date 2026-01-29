using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PartnershipManager.API.Extensions;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Infrastructure.Jobs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERILOG
// =====================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// =====================================================
// SERVICES
// =====================================================
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================

// Exception handling (primeiro da pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (sempre habilitado)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Partnership Manager API v1");
    c.RoutePrefix = "swagger";
});

// HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("DefaultPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Client and Company Context (após autenticação)
app.UseClientContext();
app.UseCompanyContext();

// Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Hangfire Dashboard - Temporarily disabled
// app.MapHangfireDashboard("/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// });

// Map Controllers
app.MapControllers();

// Configure recurring jobs
// HangfireJobsConfiguration.ConfigureRecurringJobs();

// =====================================================
// ROOT ENDPOINT
// =====================================================
app.MapGet("/", () => Results.Ok(new
{
    Name = "Partnership Manager API",
    Version = "1.0.0",
    Status = "Running",
    Documentation = "/swagger",
    HealthCheck = "/health",
    Jobs = "/hangfire"
}));

// =====================================================
// RUN
// =====================================================
Log.Information("Partnership Manager API starting...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Filtro de autorização para Hangfire Dashboard
/// </summary>
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Em desenvolvimento, permitir acesso local
        if (httpContext.Request.Host.Host == "localhost")
            return true;
            
        // Em produção, verificar se usuário está autenticado e é admin
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
