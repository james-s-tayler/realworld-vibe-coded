using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Server.Web.Users;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Articles;

public class ArticlesFixture : AppFixture<Program>
{
  private MsSqlContainer? _container;

  public HttpClient ArticlesUser1Client { get; private set; } = null!;
  public HttpClient ArticlesUser2Client { get; private set; } = null!;
  public string ArticlesUser1Username { get; private set; } = null!;
  public string ArticlesUser2Username { get; private set; } = null!;
  public string ArticlesUser1Email { get; private set; } = null!;
  public string ArticlesUser2Email { get; private set; } = null!;

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

    await db.Database.EnsureCreatedAsync();

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
        Password = articlesUser1Password
      }
    };

    var (_, registerResult1) = await Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(registerRequest1);
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
        Password = articlesUser2Password
      }
    };

    var (_, registerResult2) = await Client.POSTAsync<Server.Web.Users.Register, RegisterRequest, RegisterResponse>(registerRequest2);
    var token2 = registerResult2.User.Token;

    ArticlesUser2Client = CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token2);
    });
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
