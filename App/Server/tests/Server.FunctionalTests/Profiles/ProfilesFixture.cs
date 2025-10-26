using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Profiles;

public class ProfilesFixture : AppFixture<Program>
{
  private MsSqlContainer _container = null!;
  private string _connectionString = null!;

  protected override async ValueTask PreSetupAsync()
  {
    _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    await _container.StartAsync();

    _connectionString = _container.GetConnectionString();

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    using var serviceProvider = serviceCollection.BuildServiceProvider();
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

    // Remove Identity stores that depend on the old DbContext
    var identityStores = services.Where(d =>
        d.ServiceType == typeof(IUserStore<User>) ||
        d.ServiceType == typeof(IRoleStore<IdentityRole<Guid>>))
        .ToList();

    foreach (var desc in identityStores)
    {
      services.Remove(desc);
    }

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    // Re-register Entity Framework stores to connect Identity to the new DbContext
    services.AddScoped<IUserStore<User>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<User, IdentityRole<Guid>, AppDbContext, Guid>>();
    services.AddScoped<IRoleStore<IdentityRole<Guid>>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole<Guid>, AppDbContext, Guid>>();
  }

  protected override ValueTask SetupAsync()
  {
    return ValueTask.CompletedTask;
  }

  protected override ValueTask TearDownAsync()
  {
    return ValueTask.CompletedTask;
  }
}
