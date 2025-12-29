using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Users;

public class UsersFixture : ApiFixtureBase<Program>
{
  private MsSqlContainer _container = null!;
  private string _connectionString = null!;

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
        (d.ServiceType.ToString().Contains("AppDbContext") ||
         d.ServiceType.ToString().Contains("TenantStoreDbContext") ||
         d.ServiceType == typeof(DbContextOptions) ||
         (d.ServiceType.IsGenericType &&
          d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))) &&
        !d.ServiceType.ToString().Contains("MultiTenant"))
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

  protected override ValueTask SetupAsync()
  {
    // No longer need to create schema here - it's done once in PreSetupAsync
    return ValueTask.CompletedTask;
  }

  protected override ValueTask TearDownAsync()
  {
    // No need to dispose the container when WAF caching is enabled.
    // TestContainers will automatically dispose it when the test run finishes.
    return ValueTask.CompletedTask;
  }
}
