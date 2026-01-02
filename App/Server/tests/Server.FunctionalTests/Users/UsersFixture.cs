using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests.Users;

public class UsersFixture : ApiFixtureBase
{
  protected override async ValueTask SetupAsync()
  {
    // Apply migrations to ensure database schema is up to date
    var dbContextOptions = Services.GetRequiredService<DbContextOptions<AppDbContext>>();
    var multiTenantContextAccessor = new AsyncLocalMultiTenantContextAccessor<TenantInfo>();
    using var db = new AppDbContext(multiTenantContextAccessor, dbContextOptions, null);
    await db.Database.MigrateAsync();
  }
}
