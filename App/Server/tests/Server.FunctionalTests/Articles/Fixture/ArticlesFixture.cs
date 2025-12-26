using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Articles.Fixture;

public class ArticlesFixture : ApiFixtureBase<Program>
{
  private MsSqlContainer _container = null!;
  private string _connectionString = null!;

  public HttpClient ArticlesUser1Client { get; private set; } = null!;

  public HttpClient ArticlesUser2Client { get; private set; } = null!;

  public string ArticlesUser1Username { get; private set; } = null!;

  public string ArticlesUser2Username { get; private set; } = null!;

  public string ArticlesUser1Email { get; private set; } = null!;

  public string ArticlesUser2Email { get; private set; } = null!;

  protected override async ValueTask PreSetupAsync()
  {
    // PreSetupAsync is called once per test assembly, before the WAF/SUT is created.
    // This ensures a single container and schema for all tests using this fixture.
    _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    await _container.StartAsync();

    _connectionString = _container.GetConnectionString();

    // Create database schema once per assembly
    // Services is not available yet, so create a temporary service provider
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    await using var serviceProvider = serviceCollection.BuildServiceProvider();

    // AppDbContext constructor requires IDomainEventDispatcher but it's nullable,
    // so we can create it with a null DbContextOptions
    var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    await using var db = new AppDbContext(null!, dbContextOptions, null);
    await db.Database.MigrateAsync();
  }

  protected override void ConfigureServices(IServiceCollection services)
  {
    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
    if (descriptor != null)
    {
      services.Remove(descriptor);
    }

    var toRemove = services.Where(d =>
        d.ServiceType.ToString().Contains("AppDbContext") ||
        d.ServiceType == typeof(DbContextOptions) ||
        (d.ServiceType.IsGenericType &&
         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
        .ToList();

    foreach (var desc in toRemove)
    {
      services.Remove(desc);
    }

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });
  }

  protected override async ValueTask SetupAsync()
  {
    ArticlesUser1Email = $"articlesuser1-{Guid.NewGuid()}@example.com";
    ArticlesUser1Username = ArticlesUser1Email;
    var articlesUser1Password = "Password123!";

    ArticlesUser2Email = $"articlesuser2-{Guid.NewGuid()}@example.com";
    ArticlesUser2Username = ArticlesUser2Email;
    var articlesUser2Password = "Password123!";

    var token1 = await RegisterUserAsync(
      ArticlesUser1Email,
      articlesUser1Password);

    ArticlesUser1Client = CreateAuthenticatedClient(token1);

    var token2 = await RegisterUserAsync(
      ArticlesUser2Email,
      articlesUser2Password);

    ArticlesUser2Client = CreateAuthenticatedClient(token2);
  }

  protected override ValueTask TearDownAsync()
  {
    // No need to dispose the container when WAF caching is enabled.
    // TestContainers will automatically dispose it when the test run finishes.
    return ValueTask.CompletedTask;
  }
}
