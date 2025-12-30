using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Server.Core.TenantInfoAggregate;

namespace Server.Infrastructure.Data;

/// <summary>
/// Database context for storing tenant information.
/// This is NOT a multi-tenant context itself - it contains information about ALL tenants.
/// Typically uses a separate database/connection string from the application databases.
/// </summary>
public class TenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
{
  public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Additional configuration for TenantInfo can be added here if needed
  }
}
