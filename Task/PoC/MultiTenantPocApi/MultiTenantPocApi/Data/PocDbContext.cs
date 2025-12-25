using Audit.EntityFramework;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Data;

/// <summary>
/// POC DbContext that demonstrates MultiTenant + Identity + Audit.NET integration
/// Mimics the real App/Server/Infrastructure/Data/AppDbContext.cs structure
/// </summary>
[AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false)]
public class PocDbContext : MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PocDbContext(DbContextOptions<PocDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Article entity
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Finbuckle automatically adds TenantId filtering via [MultiTenant] attribute
            entity.HasIndex(e => e.TenantId);
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
