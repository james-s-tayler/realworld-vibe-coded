using Audit.EntityFramework;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Data;

/// <summary>
/// POC DbContext that demonstrates MultiTenant + Identity + Audit.NET integration
/// Mimics the real App/Server/Infrastructure/Data/AppDbContext.cs structure
/// Uses Finbuckle v10's MultiTenantIdentityDbContext with Identity support
/// </summary>
[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class PocDbContext : MultiTenantIdentityDbContext<ApplicationUser>
{
    // CRITICAL: In Finbuckle v10, MultiTenantIdentityDbContext constructor takes:
    // - IMultiTenantContextAccessor (non-generic!) - provides access to current tenant via DI
    // - DbContextOptions
    public PocDbContext(
        IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions<PocDbContext> options)
        : base(multiTenantContextAccessor, options)
    {
        // v10 workaround: EnforceMultiTenantOnTracking() ensures attached entities respect tenant filters
        // Call this ONLY if TenantInfo is available (otherwise it throws)
        if (TenantInfo != null)
        {
            this.EnforceMultiTenantOnTracking();
        }
    }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure EntityBase - all entities inheriting from it will be multi-tenant
        modelBuilder.Entity<EntityBase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(64); // Finbuckle v10 removed char limit, but good practice
            
            // CRITICAL: Apply IsMultiTenant() to the BASE entity type
            // EF Core query filters must be applied to the root of the inheritance hierarchy
            // This filter will automatically apply to all derived types (Article, etc.)
            entity.IsMultiTenant();
            
            // Add index on TenantId for query performance
            entity.HasIndex(e => e.TenantId);
        });

        // Configure Article entity - inherits multitenant query filter from EntityBase
        modelBuilder.Entity<Article>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            // Note: No IsMultiTenant() call here - filter is inherited from EntityBase
        });
        
        // IMPORTANT: Exclude Identity entities from multi-tenant filtering
        // In Finbuckle v10, Identity entities are multi-tenant by default on MultiTenantIdentityDbContext
        // BUT for POC registration flow without tenant context, we need to opt them out
        modelBuilder.Entity<ApplicationUser>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityRole>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityUserClaim<string>>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityUserRole<string>>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityUserLogin<string>>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityRoleClaim<string>>().IsNotMultiTenant();
        modelBuilder.Entity<IdentityUserToken<string>>().IsNotMultiTenant();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Log tenant context for debugging
        if (TenantInfo != null)
        {
            Console.WriteLine($"[SaveChanges] Operating in tenant: {TenantInfo.Id} ({TenantInfo.Name})");
        }
        else
        {
            Console.WriteLine("[SaveChanges] No tenant context - allowing operation (likely Identity operation)");
        }
        
        // Audit.NET integration - captures changes with tenant context
        // The [AuditDbContext] attribute handles the audit logging automatically
        
        // Let base class handle multi-tenant enforcement
        // It will only apply to entities marked as multi-tenant (Article), not Identity entities (IsNotMultiTenant)
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
