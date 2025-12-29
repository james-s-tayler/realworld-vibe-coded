using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Profiles;

public class ProfilesFixture : ApiFixtureBase<Program>
{
  private MsSqlContainer _container = null!;
  private string _connectionString = null!;

  public HttpClient AuthenticatedClient { get; private set; } = null!;

  protected override async ValueTask PreSetupAsync()
  {
    _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    await _container.StartAsync();

    _connectionString = _container.GetConnectionString();

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IMultiTenantContextAccessor<TenantInfo>>(new AsyncLocalMultiTenantContextAccessor<TenantInfo>());
    serviceCollection.AddDbContext<TenantStoreDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    using var serviceProvider = serviceCollection.BuildServiceProvider();

    // Apply TenantStore migrations first
    using var tenantStoreDb = serviceProvider.GetRequiredService<TenantStoreDbContext>();
    await tenantStoreDb.Database.MigrateAsync();

    // Then apply AppDbContext migrations
    using var db = serviceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
  }

  protected override void ConfigureServices(IServiceCollection services)
  {
    var toRemove = services.Where(d =>
        d.ServiceType.ToString().Contains("AppDbContext") ||
        d.ServiceType.ToString().Contains("TenantStoreDbContext") ||
        d.ServiceType == typeof(DbContextOptions) ||
        (d.ServiceType.IsGenericType &&
         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
        .ToList();

    foreach (var desc in toRemove)
    {
      services.Remove(desc);
    }

    services.AddDbContext<TenantStoreDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });
  }

  protected override async ValueTask SetupAsync()
  {
    // Create an authenticated client for tests that need authentication
    var email = $"testuser-{Guid.NewGuid()}@example.com";
    var password = "Password123!";
    var token = await RegisterUserAsync(email, password);
    AuthenticatedClient = CreateAuthenticatedClient(token);
  }

  protected override ValueTask TearDownAsync()
  {
    return ValueTask.CompletedTask;
  }
}
