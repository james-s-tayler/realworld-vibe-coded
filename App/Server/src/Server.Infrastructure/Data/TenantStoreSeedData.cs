using Server.Core.TenantAggregate;

namespace Server.Infrastructure.Data;

public static class TenantStoreSeedData
{
  public static async Task InitializeAsync(TenantStoreDbContext dbContext)
  {
    // Ensure database exists and is up to date
    await dbContext.Database.MigrateAsync();

    // Check if we already have tenants
    if (await dbContext.Set<TenantInfo>().AnyAsync())
    {
      return; // Already seeded
    }

    // Add default tenant for development/testing
    var defaultTenant = new TenantInfo(
      id: "default-tenant",
      identifier: "default",
      name: "Default Tenant");

    dbContext.Set<TenantInfo>().Add(defaultTenant);
    await dbContext.SaveChangesAsync();
  }
}
