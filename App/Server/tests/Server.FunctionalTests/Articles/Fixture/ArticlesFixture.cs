using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests.Articles.Fixture;

public class ArticlesFixture : ApiFixtureBase
{
  public HttpClient ArticlesUser1Client { get; private set; } = null!;

  public HttpClient ArticlesUser2Client { get; private set; } = null!;

  public string ArticlesUser1Username { get; private set; } = null!;

  public string ArticlesUser2Username { get; private set; } = null!;

  public string ArticlesUser1Email { get; private set; } = null!;

  public string ArticlesUser2Email { get; private set; } = null!;

  protected override async ValueTask SetupAsync()
  {
    // Apply migrations to ensure database schema is up to date
    var dbContextOptions = Services.GetRequiredService<DbContextOptions<AppDbContext>>();
    var multiTenantContextAccessor = new AsyncLocalMultiTenantContextAccessor<TenantInfo>();
    using var db = new AppDbContext(multiTenantContextAccessor, dbContextOptions, null);
    await db.Database.MigrateAsync();

    ArticlesUser1Email = $"articlesuser1-{Guid.NewGuid()}@example.com";
    ArticlesUser1Username = ArticlesUser1Email;
    var articlesUser1Password = "Password123!";

    ArticlesUser2Email = $"articlesuser2-{Guid.NewGuid()}@example.com";
    ArticlesUser2Username = ArticlesUser2Email;
    var articlesUser2Password = "Password123!";

    var token1 = await RegisterTenantUserAsync(
      ArticlesUser1Email,
      articlesUser1Password);

    ArticlesUser1Client = CreateAuthenticatedClient(token1);

    var token2 = await RegisterTenantUserAsync(
      ArticlesUser2Email,
      articlesUser2Password);

    ArticlesUser2Client = CreateAuthenticatedClient(token2);
  }
}
