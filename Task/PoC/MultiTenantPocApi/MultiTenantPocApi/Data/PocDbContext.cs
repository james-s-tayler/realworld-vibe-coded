using Audit.EntityFramework;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Data;

/// <summary>
/// POC DbContext that demonstrates MultiTenant + Identity + Audit.NET integration
/// Mimics the real App/Server/Infrastructure/Data/AppDbContext.cs structure
/// Uses Finbuckle v10's MultiTenantDbContext with Identity support
/// </summary>
[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class PocDbContext : MultiTenantDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PocDbContext(
        DbContextOptions<PocDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity entities
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.IsMultiTenant();
        });

        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.IsMultiTenant();
        });

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
        // Apply multi-tenant logic before saving
        this.EnforceMultiTenant();
        
        // Audit.NET integration - captures changes with tenant context
        // The [AuditDbContext] attribute handles the audit logging automatically
        
        // Log tenant context for demonstration
        var tenantInfo = _httpContextAccessor?.HttpContext?.GetMultiTenantContext<Finbuckle.MultiTenant.Abstractions.ITenantInfo>()?.TenantInfo;
        if (tenantInfo != null)
        {
            Console.WriteLine($"[SaveChanges] Operating in tenant: {tenantInfo.Id} ({tenantInfo.Name})");
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
