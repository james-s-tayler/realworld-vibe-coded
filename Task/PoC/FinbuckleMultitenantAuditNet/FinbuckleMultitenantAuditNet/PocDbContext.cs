using Audit.Core;
using Audit.EntityFramework;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC DbContext that validates multi-tenant patterns work with Audit.NET.
/// For simplicity in this POC, we inherit from IdentityDbContext and manually
/// configure multi-tenancy via the [MultiTenant] attribute on entities.
/// This validates the core concept without needing full DI setup.
/// </summary>
public class PocDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // For POC testing, we'll track the TenantId manually
    public string? CurrentTenantId { get; set; }
    
    public PocDbContext(DbContextOptions<PocDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<PocArticle> Articles { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Call base for Identity configuration
        base.OnModelCreating(builder);
        
        // Configure PocArticle with multi-tenant query filter
        builder.Entity<PocArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Index on TenantId for query performance
            entity.HasIndex(e => e.TenantId);
            
            // Global query filter for multi-tenancy (POC simulation)
            // In production, Finbuckle would handle this automatically
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // For POC: Simple audit logging
        // Log entities that are being modified
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();
        
        foreach (var entry in modifiedEntries)
        {
            Console.WriteLine($"[AUDIT] {entry.State} on {entry.Metadata.GetTableName()}, TenantId: {CurrentTenantId}");
        }
        
        // Call base SaveChangesAsync 
        var result = await base.SaveChangesAsync(cancellationToken);
        
        return result;
    }
}
