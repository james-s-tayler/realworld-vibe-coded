using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Development"); // will not send real emails
    var host = builder.Build();
    host.Start();

    // Get service provider.
    var serviceProvider = host.Services;

    // Create a scope to obtain a reference to the database
    // context (AppDbContext).
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      // Reset database for each test run
      db.Database.EnsureDeleted();

      // Ensure the database is created.
      db.Database.EnsureCreated();

      try
      {
        // Can also skip creating the items
        //if (!db.ToDoItems.Any())
        //{
        // Seed the database with test data.
        SeedData.PopulateTestDataAsync(db).Wait();
        //}
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }

  private SqliteConnection? _connection;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureServices(services =>
        {
          // Remove the app's ApplicationDbContext registration completely
          var dbContextDescriptor = services.SingleOrDefault(
              d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
          if (dbContextDescriptor != null)
          {
            services.Remove(dbContextDescriptor);
          }

          // Remove the factory and other DbContext-related services
          var toRemove = services.Where(d =>
              d.ServiceType.ToString().Contains("AppDbContext") ||
              d.ServiceType == typeof(DbContextOptions) ||
              (d.ServiceType.IsGenericType &&
               d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
              .ToList();

          foreach (var descriptor in toRemove)
          {
            services.Remove(descriptor);
          }

          // Use SQLite in-memory database for functional tests
          // Note: Cannot use EF Core InMemory provider due to raw SQL queries in the codebase
          _connection = new SqliteConnection("DataSource=:memory:");
          _connection.Open();

          services.AddDbContext<AppDbContext>((sp, options) =>
          {
            options.UseSqlite(_connection);
            // Don't use internal service provider to avoid conflicts
          }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
        });
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _connection?.Close();
      _connection?.Dispose();
    }
    base.Dispose(disposing);
  }
}
