using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Articles.Fixture;

public class EmptyArticlesFixture : AppFixture<Program>
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

    // Create database schema once per assembly for both contexts
    // Services is not available yet, so create a temporary service provider
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<IdentityDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });
    serviceCollection.AddDbContext<DomainDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    using var serviceProvider = serviceCollection.BuildServiceProvider();
    
    // Run migrations for IdentityDbContext
    var identityDbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<IdentityDbContext>>();
    using var identityDb = new IdentityDbContext(identityDbContextOptions);
    await identityDb.Database.MigrateAsync();

    // Run migrations for DomainDbContext
    var domainDbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<DomainDbContext>>();
    using var domainDb = new DomainDbContext(domainDbContextOptions, null);
    await domainDb.Database.MigrateAsync();
  }

  protected override void ConfigureServices(IServiceCollection services)
  {
    // Remove existing DbContext registrations for both contexts
    var toRemove = services.Where(d =>
        d.ServiceType.ToString().Contains("IdentityDbContext") ||
        d.ServiceType.ToString().Contains("DomainDbContext") ||
        d.ServiceType == typeof(DbContextOptions) ||
        (d.ServiceType.IsGenericType &&
         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
        .ToList();

    foreach (var desc in toRemove)
    {
      services.Remove(desc);
    }

    // Remove Identity stores that depend on the old DbContext
    var identityStores = services.Where(d =>
        d.ServiceType == typeof(IUserStore<User>) ||
        d.ServiceType == typeof(IRoleStore<IdentityRole<Guid>>))
        .ToList();

    foreach (var desc in identityStores)
    {
      services.Remove(desc);
    }

    // Re-add both DbContexts with test connection string
    services.AddDbContext<IdentityDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    services.AddDbContext<DomainDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    // Re-register Entity Framework stores to connect Identity to the new IdentityDbContext
    services.AddScoped<IUserStore<User>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<User, IdentityRole<Guid>, IdentityDbContext, Guid>>();
    services.AddScoped<IRoleStore<IdentityRole<Guid>>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole<Guid>, IdentityDbContext, Guid>>();
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
