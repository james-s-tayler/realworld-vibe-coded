using Microsoft.EntityFrameworkCore.Design;

namespace Server.Infrastructure.Data;

/// <summary>
/// Design-time factory for TenantStoreDbContext to support EF Core migrations.
/// </summary>
public class TenantStoreDbContextFactory : IDesignTimeDbContextFactory<TenantStoreDbContext>
{
  public TenantStoreDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<TenantStoreDbContext>();

    // Use SQL Server for migrations (same as used in configuration)
    // Connection string for design-time migrations
    optionsBuilder.UseSqlServer(
      "Server=(localdb)\\mssqllocaldb;Database=RealWorldApp;Trusted_Connection=True;MultipleActiveResultSets=true",
      b => b.MigrationsAssembly("Server.Infrastructure")
            .MigrationsHistoryTable("__EFMigrationsHistory_TenantStore"));

    return new TenantStoreDbContext(optionsBuilder.Options);
  }
}
