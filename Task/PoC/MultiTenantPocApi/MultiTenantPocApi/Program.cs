using Audit.Core;
using FastEndpoints;
using FastEndpoints.Swagger;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;
using Serilog;
using Serilog.Events;

// Configure Serilog early (but suppress for tests)
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) 
    && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/poc-api-.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}

var builder = WebApplication.CreateBuilder(args);

// Use Serilog (but skip for tests)
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Host.UseSerilog();
}

// Configure Audit.NET to log to files
var auditLogsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Audit.NET");
if (!Directory.Exists(auditLogsPath))
{
    Directory.CreateDirectory(auditLogsPath);
}

Audit.Core.Configuration.Setup()
    .UseFileLogProvider(config => config
        .Directory(auditLogsPath)
        .FilenameBuilder(auditEvent =>
            $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.json"))
    .WithCreationPolicy(EventCreationPolicy.InsertOnEnd);

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Add Authorization (required by ASP.NET Core)
builder.Services.AddAuthorization();

// Add HttpContextAccessor for Audit.NET
builder.Services.AddHttpContextAccessor();

// Add Multi-Tenant support - using TenantInfo record directly (v10 pattern)
builder.Services.AddMultiTenant<TenantInfo>()
    // Strategy: Extract tenant ID from X-Tenant-Id header
    .WithDelegateStrategy(context =>
    {
        if (context is HttpContext httpContext && 
            httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
        {
            var tenantId = tenantIdValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return Task.FromResult<string?>(tenantId);
            }
        }
        return Task.FromResult<string?>(null);
    })
    // Store: Echo Store creates TenantInfo on-the-fly from tenant identifier (perfect for POC/testing)
    // In production, use WithEFCoreStore() or WithConfigurationStore() for real tenant validation
    .WithEchoStore();

// Add DbContext with Multi-Tenant support
builder.Services.AddDbContext<PocDbContext>((serviceProvider, options) =>
{
    // Use in-memory database for POC
    // IMPORTANT: Use consistent name in tests so all requests share the same database instance
    // The database is still isolated per test fixture instance
    options.UseInMemoryDatabase("PocDb");
    
    // Enable sensitive data logging for debugging (test environment only)
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

var app = builder.Build();

// Use Multi-Tenant middleware (must be before routing)
app.UseMultiTenant();

app.UseAuthorization();

// Use FastEndpoints
app.UseFastEndpoints();

// Use Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

if (builder.Environment.EnvironmentName != "Testing")
{
    Log.Information("POC API Started");
    Log.Information("Use X-Tenant-Id header to specify tenant (tenant-1 or tenant-2)");
    Log.Information("Endpoints:");
    Log.Information("  POST /api/articles - Create article");
    Log.Information("  GET /api/articles - List articles (tenant-scoped)");
    Log.Information("Logs:");
    Log.Information("  Serilog: Logs/poc-api-*.log");
    Log.Information("  Audit.NET: Logs/Audit.NET/*.json");
}

app.Run();

if (builder.Environment.EnvironmentName != "Testing")
{
    Log.CloseAndFlush();
}

// Make Program class public for testing
public partial class Program
{
}
