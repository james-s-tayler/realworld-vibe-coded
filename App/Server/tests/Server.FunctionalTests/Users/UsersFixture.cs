using Microsoft.EntityFrameworkCore;
using Server.FunctionalTests.Infrastructure;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests.Users;

public class UsersFixture : AppFixture<Program>
{
  private string _connectionString = null!;

  protected override async ValueTask PreSetupAsync()
  {
    // Use shared SQL Server container instead of creating a new one
    _connectionString = await SharedSqlServerContainer.GetConnectionStringAsync();

    // Create database schema once per fixture
    // Services is not available yet, so create a temporary service provider
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    using var serviceProvider = serviceCollection.BuildServiceProvider();

    // AppDbContext constructor requires IDomainEventDispatcher but it's nullable,
    // so we can create it with a null DbContextOptions
    var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    using var db = new AppDbContext(dbContextOptions, null);
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

  protected override ValueTask SetupAsync()
  {
    // Schema is already created in PreSetupAsync
    return ValueTask.CompletedTask;
  }

  protected override ValueTask TearDownAsync()
  {
    // Shared container will be disposed automatically by Testcontainers
    return ValueTask.CompletedTask;
  }
}
