using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithPassword("YourStrong@Passw0rd")
    .Build();

  private bool _isInitialized = false;
  private readonly SemaphoreSlim _initLock = new(1, 1);

  public async Task InitializeAsync()
  {
    await _msSqlContainer.StartAsync();
  }

  public new async Task DisposeAsync()
  {
    await _msSqlContainer.StopAsync();
    await _msSqlContainer.DisposeAsync();
    _initLock.Dispose();
  }

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

    // Initialize database only once across all test classes
    _initLock.Wait();
    try
    {
      if (!_isInitialized)
      {
        // Get service provider.
        var serviceProvider = host.Services;

        // Create a scope to obtain a reference to the database context (AppDbContext).
        using (var scope = serviceProvider.CreateScope())
        {
          var scopedServices = scope.ServiceProvider;
          var db = scopedServices.GetRequiredService<AppDbContext>();

          var logger = scopedServices
              .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

          // Ensure the database is created.
          db.Database.EnsureCreated();

          try
          {
            // Seed the database with test data.
            SeedData.PopulateTestDataAsync(db).Wait();
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "An error occurred seeding the " +
                                "database with test messages. Error: {exceptionMessage}", ex.Message);
          }
        }

        _isInitialized = true;
      }
    }
    finally
    {
      _initLock.Release();
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureServices(services =>
        {
          // Remove the app's ApplicationDbContext registration.
          var descriptor = services.SingleOrDefault(
          d => d.ServiceType ==
              typeof(DbContextOptions<AppDbContext>));

          if (descriptor != null)
          {
            services.Remove(descriptor);
          }

          // Remove other DbContext-related services to avoid conflicts
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

          // Add ApplicationDbContext using SQL Server from Testcontainers
          // Add Database=TestDb to the connection string to avoid using master
          var connectionString = _msSqlContainer.GetConnectionString() + ";Database=TestDb";
          services.AddDbContext<AppDbContext>(options =>
          {
            options.UseSqlServer(connectionString);
          });
        });
  }
}
