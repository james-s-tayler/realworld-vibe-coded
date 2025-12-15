using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Server.FunctionalTests.Infrastructure;
using Server.Infrastructure.Data;
using Server.Web.Users.Register;

namespace Server.FunctionalTests.Articles.Fixture;

public class ArticlesFixture : AppFixture<Program>
{
  private string _connectionString = null!;

  public HttpClient ArticlesUser1Client { get; private set; } = null!;

  public HttpClient ArticlesUser2Client { get; private set; } = null!;

  public string ArticlesUser1Username { get; private set; } = null!;

  public string ArticlesUser2Username { get; private set; } = null!;

  public string ArticlesUser1Email { get; private set; } = null!;

  public string ArticlesUser2Email { get; private set; } = null!;

  protected override async ValueTask PreSetupAsync()
  {
    // Use shared SQL Server container
    _connectionString = await SharedSqlServerContainer.GetConnectionStringAsync();

    // Create database schema
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(_connectionString);
      options.EnableSensitiveDataLogging();
    });

    await using var serviceProvider = serviceCollection.BuildServiceProvider();

    // AppDbContext constructor requires IDomainEventDispatcher but it's nullable,
    // so we can create it with a null DbContextOptions
    var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    await using var db = new AppDbContext(dbContextOptions, null);
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

  protected override async ValueTask SetupAsync()
  {
    // Schema is already created in PreSetupAsync
    // SetupAsync still needs to run to create test data (users, clients, etc.)

    ArticlesUser1Username = $"articlesuser1-{Guid.NewGuid()}";
    ArticlesUser1Email = $"articlesuser1-{Guid.NewGuid()}@example.com";
    var articlesUser1Password = "password123";

    ArticlesUser2Username = $"articlesuser2-{Guid.NewGuid()}";
    ArticlesUser2Email = $"articlesuser2-{Guid.NewGuid()}@example.com";
    var articlesUser2Password = "password123";

    var registerRequest1 = new RegisterRequest
    {
      User = new UserData
      {
        Email = ArticlesUser1Email,
        Username = ArticlesUser1Username,
        Password = articlesUser1Password,
      },
    };

    var (_, registerResult1) = await Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest1);
    var token1 = registerResult1.User.Token;

    ArticlesUser1Client = CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token1);
    });

    var registerRequest2 = new RegisterRequest
    {
      User = new UserData
      {
        Email = ArticlesUser2Email,
        Username = ArticlesUser2Username,
        Password = articlesUser2Password,
      },
    };

    var (_, registerResult2) = await Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest2);
    var token2 = registerResult2.User.Token;

    ArticlesUser2Client = CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token2);
    });
  }

  protected override ValueTask TearDownAsync()
  {
    // No need to dispose the container when WAF caching is enabled.
    // TestContainers will automatically dispose it when the test run finishes.
    return ValueTask.CompletedTask;
  }
}
