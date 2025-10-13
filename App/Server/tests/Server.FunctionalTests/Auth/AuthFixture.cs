using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Auth;

public class AuthFixture : AppFixture<Program>
{
  private MsSqlContainer? _container;

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

    _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    _container.StartAsync().GetAwaiter().GetResult();

    var connectionString = _container.GetConnectionString();

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(connectionString);
      options.EnableSensitiveDataLogging();
    });
  }

  protected override async ValueTask SetupAsync()
  {
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Create database schema (uses model from DbContext, not migrations)
    await db.Database.EnsureCreatedAsync();
  }

  protected override async ValueTask TearDownAsync()
  {
    if (_container != null)
    {
      await _container.StopAsync();
      await _container.DisposeAsync();
    }
  }
}
