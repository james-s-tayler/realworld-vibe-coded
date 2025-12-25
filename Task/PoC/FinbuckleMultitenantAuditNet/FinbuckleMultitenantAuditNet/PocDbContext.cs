using Audit.Core;
using Audit.EntityFramework;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC DbContext that validates MultiTenantIdentityDbContext works with Audit.NET.
/// This inherits from MultiTenantIdentityDbContext (Finbuckle) to validate the actual
/// production approach, demonstrating that multi-tenant filtering and audit logging
/// work together without conflicts.
/// 
/// Note: For testing, we use MultiTenantDbContext.Create factory method rather than
/// constructor injection to avoid dependency injection setup complexity in the POC.
/// </summary>
public class PocDbContext : MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // Note: MultiTenantIdentityDbContext requires IMultiTenantContextAccessor in constructor,
    // but for testing we'll use the factory method pattern
    public PocDbContext(DbContextOptions<PocDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<PocArticle> Articles { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Call base to get Finbuckle's automatic multi-tenant configuration
        base.OnModelCreating(builder);
        
        // Configure PocArticle entity
        builder.Entity<PocArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Index on TenantId for query performance
            entity.HasIndex(e => e.TenantId);
        });
        
        // Note: Finbuckle automatically applies [MultiTenant] to the PocArticle entity
        // because of the [MultiTenant] attribute on the class, which adds the query filter
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // For POC: Simple audit logging with tenant context
        // In production, this would use Audit.NET's data provider pattern
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();
        
        // Get current tenant info (Finbuckle provides this via TenantInfo property)
        var currentTenantId = TenantInfo?.Id ?? "unknown";
        
        foreach (var entry in modifiedEntries)
        {
            Console.WriteLine($"[AUDIT] {entry.State} on {entry.Metadata.GetTableName()}, TenantId: {currentTenantId}");
        }
        
        // Call base SaveChangesAsync - Finbuckle automatically associates entities with TenantId
        var result = await base.SaveChangesAsync(cancellationToken);
        
        return result;
    }
}
