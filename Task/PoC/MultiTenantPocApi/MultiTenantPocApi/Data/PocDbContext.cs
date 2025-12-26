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
        // Recommended workaround for v10 attached entity tracking issue
        this.EnforceMultiTenantOnTracking();
    }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        
        // NOTE: Identity entity types (ApplicationUser, IdentityRole, etc.) are automatically
        // multi-tenant by default in MultiTenantIdentityDbContext in Finbuckle v10
        // No need to explicitly call IsMultiTenant() on them
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Apply multi-tenant logic before saving
        this.EnforceMultiTenant();
        
        // Audit.NET integration - captures changes with tenant context
        // The [AuditDbContext] attribute handles the audit logging automatically
        
        // Log tenant context for demonstration
        var tenantInfo = _httpContextAccessor?.HttpContext?.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        if (tenantInfo != null)
        {
            Console.WriteLine($"[SaveChanges] Operating in tenant: {tenantInfo.Id} ({tenantInfo.Name})");
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
