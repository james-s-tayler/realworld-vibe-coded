using Audit.Core;
using FastEndpoints;
using FastEndpoints.Swagger;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;
using MultiTenantPocApi.Models;
using Serilog;
using Serilog.Events;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/poc-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting POC API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

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
        .WithCreationPolicy(EventCreationPolicy.InsertOnEnd)
        .WithAction(action => action
            .OnScopeCreated(scope =>
            {
                // Add tenant information to audit events
                var httpContext = builder.Services.BuildServiceProvider().GetService<IHttpContextAccessor>()?.HttpContext;
                if (httpContext != null)
                {
                    var tenantInfo = httpContext.GetMultiTenantContext<Finbuckle.MultiTenant.Abstractions.ITenantInfo>()?.TenantInfo;
                    if (tenantInfo != null)
                    {
                        scope.SetCustomField("TenantId", tenantInfo.Id);
                        scope.SetCustomField("TenantName", tenantInfo.Name);
                    }
                }
            }));

    // Add FastEndpoints
    builder.Services.AddFastEndpoints();
    builder.Services.SwaggerDocument();

    // Add HttpContextAccessor for Audit.NET
    builder.Services.AddHttpContextAccessor();

    // Add Multi-Tenant support
    builder.Services.AddMultiTenant<Finbuckle.MultiTenant.Abstractions.ITenantInfo>()
        .WithInMemoryStore(options =>
        {
            // Configure two tenants for POC
            options.Tenants.Add(new Finbuckle.MultiTenant.Abstractions.TenantInfo
            {
                Id = "tenant-1",
                Identifier = "tenant-1",
                Name = "Tenant One"
            });
            options.Tenants.Add(new Finbuckle.MultiTenant.Abstractions.TenantInfo
            {
                Id = "tenant-2",
                Identifier = "tenant-2",
                Name = "Tenant Two"
            });
        })
        .WithHeaderStrategy("X-Tenant-Id"); // Use header-based tenant resolution for POC

    // Add DbContext with Multi-Tenant support
    builder.Services.AddDbContext<PocDbContext>((serviceProvider, options) =>
    {
        // Use in-memory database for POC
        options.UseInMemoryDatabase("PocDb");
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

    Log.Information("POC API Started");
    Log.Information("Use X-Tenant-Id header to specify tenant (tenant-1 or tenant-2)");
    Log.Information("Endpoints:");
    Log.Information("  POST /api/articles - Create article");
    Log.Information("  GET /api/articles - List articles (tenant-scoped)");
    Log.Information("Logs:");
    Log.Information("  Serilog: Logs/poc-api-*.log");
    Log.Information("  Audit.NET: Logs/Audit.NET/*.json");

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

// Make Program class public for testing
public partial class Program { }
