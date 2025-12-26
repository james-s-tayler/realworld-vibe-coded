using Audit.Core;
using FastEndpoints;
using FastEndpoints.Swagger;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;
using MultiTenantPocApi.Models;
using MultiTenantPocApi.Services;
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

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configure password requirements for POC (relaxed for testing)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<PocDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// Add Claims Transformation to add TenantId claim
builder.Services.AddScoped<IClaimsTransformation, TenantClaimsTransformation>();

// Add Multi-Tenant support - using TenantInfo record directly (v10 pattern)
builder.Services.AddMultiTenant<TenantInfo>()
    // Strategy: Use ClaimStrategy to read TenantId from authenticated user's claims
    // This is the production pattern - tenant comes from user's identity
    .WithClaimStrategy("TenantId")
    // Store: InMemoryStore with predefined tenants for POC
    // In production, use WithEFCoreStore() for database-backed tenant configuration
    .WithInMemoryStore(options =>
    {
        options.Tenants.Add(new TenantInfo("tenant-1", "tenant-1", "Tenant One"));
        options.Tenants.Add(new TenantInfo("tenant-2", "tenant-2", "Tenant Two"));
    });

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

// CRITICAL: Use Multi-Tenant middleware BEFORE authentication
// This is required for ClaimStrategy to work - tenant resolution happens first,
// then authentication adds claims (including TenantId via IClaimsTransformation)
app.UseMultiTenant();

// Use authentication and authorization
app.UseAuthentication();
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
    Log.Information("POC API Started - Multi-Tenant with ClaimStrategy + Identity");
    Log.Information("Authentication:");
    Log.Information("  POST /api/auth/register - Register new user with TenantId");
    Log.Information("  POST /api/auth/login - Login to get authentication cookie");
    Log.Information("Endpoints:");
    Log.Information("  POST /api/articles - Create article (requires authentication)");
    Log.Information("  GET /api/articles - List articles (tenant-scoped, requires authentication)");
    Log.Information("Tenants:");
    Log.Information("  tenant-1 - Tenant One");
    Log.Information("  tenant-2 - Tenant Two");
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
