using Audit.EntityFramework;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Data;

/// <summary>
/// POC DbContext that demonstrates MultiTenant + Identity + Audit.NET integration
/// Mimics the real App/Server/Infrastructure/Data/AppDbContext.cs structure
/// Uses Finbuckle v10's MultiTenantIdentityDbContext with proper dependency injection
/// </summary>
[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class PocDbContext : MultiTenantIdentityDbContext<ApplicationUser>
{
    public PocDbContext(DbContextOptions<PocDbContext> options)
        : base(options)
    {
        // Workaround for v10 attached entity tracking issue
        // Will be included in base class in future versions
        EnforceMultiTenantOnTracking();
    }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mark ApplicationUser as multi-tenant (Identity entities are multi-tenant by default in v10)
        modelBuilder.Entity<ApplicationUser>().IsMultiTenant();

        // Configure EntityBase as multi-tenant - all entities inheriting from it will be multi-tenant
        modelBuilder.Entity<EntityBase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Mark as multi-tenant - this applies to all entities inheriting from EntityBase
            entity.IsMultiTenant();
            entity.HasIndex(e => e.TenantId);
        });

        // Configure Article entity - inherits multitenant configuration from EntityBase
        modelBuilder.Entity<Article>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit.NET integration - captures changes with tenant context
        // The [AuditDbContext] attribute handles the audit logging automatically
        
        // Log tenant context for demonstration
        if (TenantInfo != null)
        {
            Console.WriteLine($"[SaveChanges] Operating in tenant: {TenantInfo.Id} ({TenantInfo.Name})");
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
