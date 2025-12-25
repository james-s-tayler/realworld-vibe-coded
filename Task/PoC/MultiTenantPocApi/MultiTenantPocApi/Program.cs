using Audit.Core;
using FastEndpoints;
using FastEndpoints.Swagger;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;
using MultiTenantPocApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Audit.NET to log to console
Audit.Core.Configuration.Setup()
    .UseCustomProvider(new ConsoleAuditDataProvider());

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Add Multi-Tenant support
builder.Services.AddMultiTenant<PocTenantInfo>()
    .WithInMemoryStore(options =>
    {
        // Configure two tenants for POC
        options.Tenants.Add(new PocTenantInfo
        {
            Id = "tenant-1",
            Identifier = "tenant-1",
            Name = "Tenant One"
        });
        options.Tenants.Add(new PocTenantInfo
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

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<PocDbContext>()
    .AddDefaultTokenProviders();

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

Console.WriteLine("POC API Started");
Console.WriteLine("Use X-Tenant-Id header to specify tenant (tenant-1 or tenant-2)");
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST /api/articles - Create article");
Console.WriteLine("  GET /api/articles - List articles (tenant-scoped)");

app.Run();

// Custom audit data provider for console output
public class ConsoleAuditDataProvider : AuditDataProvider
{
    public override object InsertEvent(AuditEvent auditEvent)
    {
        Console.WriteLine($"[AUDIT] {auditEvent.EventType} - Duration: {auditEvent.Duration}ms");
        if (auditEvent.CustomFields.ContainsKey("TenantId"))
        {
            Console.WriteLine($"[AUDIT] TenantId: {auditEvent.CustomFields["TenantId"]}");
        }
        return Guid.NewGuid();
    }

    public override Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(InsertEvent(auditEvent));
    }

    public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
    {
        // Not used
    }

    public override Task ReplaceEventAsync(object eventId, AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
