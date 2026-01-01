namespace Server.FunctionalTests.Profiles;

public class ProfilesFixture : ApiFixtureBase
{
  public HttpClient AuthenticatedClient { get; private set; } = null!;

  protected override async ValueTask SetupAsync()
  {
    // Create an authenticated client for tests that need authentication
    var email = $"testuser-{Guid.NewGuid()}@example.com";
    var password = "Password123!";
    var token = await RegisterTenantUserAsync(email, password);
    AuthenticatedClient = CreateAuthenticatedClient(token);
  }
}
