using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests.Auth;

public class AuthFixture : AppFixture<Program>
{
  private DbConnection? _connection;

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

    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlite(_connection);
      options.EnableSensitiveDataLogging();
    });
  }

  protected override async ValueTask SetupAsync()
  {
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
  }

  protected override async ValueTask TearDownAsync()
  {
    if (_connection != null)
    {
      await _connection.CloseAsync();
      await _connection.DisposeAsync();
    }
  }
}
