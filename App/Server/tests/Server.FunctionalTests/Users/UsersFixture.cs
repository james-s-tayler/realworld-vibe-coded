using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests.Users;

[Collection("SqlServer Assembly Collection")]
public class UsersFixture : AppFixture<Program>
{
  private readonly SqlServerAssemblyFixture _assemblyFixture;
  private DatabaseLease? _databaseLease;
  private string? _connectionString;

  public UsersFixture(SqlServerAssemblyFixture assemblyFixture)
  {
    _assemblyFixture = assemblyFixture;
  }

  protected override async ValueTask PreSetupAsync()
  {
    // Lease a database from the assembly fixture
    _databaseLease = await _assemblyFixture.LeaseDatabaseAsync();
    _connectionString = _databaseLease.ConnectionString;
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
    return ValueTask.CompletedTask;
  }

  protected override async ValueTask TearDownAsync()
  {
    if (_databaseLease != null)
    {
      await _databaseLease.DisposeAsync();
    }
  }
}
