namespace Server.FunctionalTests.Articles.Fixture;

public class ArticlesFixture : ApiFixture, IApiFixture
{
  public HttpClient ArticlesUser1Client { get; private set; } = null!;

  public HttpClient ArticlesUser2Client { get; private set; } = null!;

  public string ArticlesUser1Username { get; private set; } = null!;

  public string ArticlesUser2Username { get; private set; } = null!;

  public string ArticlesUser1Email { get; private set; } = null!;

  public string ArticlesUser2Email { get; private set; } = null!;

  protected override async ValueTask SetupAsync()
  {
    var tenant = await this.RegisterTenantWithUsersAsync(2);

    var user1 = tenant.Users[0];
    ArticlesUser1Email = user1.Email;
    ArticlesUser1Username = user1.Email;
    ArticlesUser1Client = user1.Client;

    var user2 = tenant.Users[1];
    ArticlesUser2Email = user2.Email;
    ArticlesUser2Username = user2.Email;
    ArticlesUser2Client = user2.Client;
  }
}
