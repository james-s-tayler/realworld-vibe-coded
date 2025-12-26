using Audit.EntityFramework;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PocDbContext(
        IMultiTenantContextAccessor<TenantInfo> multiTenantContextAccessor,
        DbContextOptions<PocDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(multiTenantContextAccessor, options)
    {
        _httpContextAccessor = httpContextAccessor;
        // NOTE: EnforceMultiTenantOnTracking() removed from constructor because it throws when TenantInfo is null
        // Instead, we'll rely on EnforceMultiTenant() in SaveChangesAsync to set TenantId
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
        
        // NOTE: Identity entity types (ApplicationUser, IdentityRole, etc.) are automatically
        // multi-tenant by default in MultiTenantIdentityDbContext in Finbuckle v10
        // No need to explicitly call IsMultiTenant() on them
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get current tenant context
        var tenantInfo = _httpContextAccessor?.HttpContext?.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        
        // CRITICAL: Apply multi-tenant logic before saving
        // This sets TenantId on all added/modified entities based on current tenant context
        // Only call EnforceMultiTenant() if we have a tenant context
        if (tenantInfo != null)
        {
            this.EnforceMultiTenant();
            Console.WriteLine($"[SaveChanges] Operating in tenant: {tenantInfo.Id} ({tenantInfo.Name})");
        }
        else
        {
            Console.WriteLine("[SaveChanges] WARNING: No tenant context - skipping EnforceMultiTenant()");
        }
        
        // Audit.NET integration - captures changes with tenant context
        // The [AuditDbContext] attribute handles the audit logging automatically

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
