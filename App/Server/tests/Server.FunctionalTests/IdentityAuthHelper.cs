namespace Server.FunctionalTests;

/// <summary>
/// Helper class for creating authenticated HttpClients using ASP.NET Core Identity endpoints
/// </summary>
public static class IdentityAuthHelper
{
  /// <summary>
  /// Creates an authenticated HttpClient by registering a new user with Identity and capturing the authentication cookie
  /// </summary>
  public static async Task<(HttpClient client, string email, string username)> CreateAuthenticatedClient<TFixture>(
      AppFixture<TFixture> fixture,
      string? email = null,
      string? password = null)
      where TFixture : class
  {
    email ??= $"user-{Guid.NewGuid()}@test.com";
    password ??= "TestPass123!";

    // Extract username from email (Identity uses email as username by default)
    var username = email.Split('@')[0];

    // Create a client with cookie handling enabled
    // SRV007: Creating client with WebApplicationFactoryClientOptions for cookie support
#pragma warning disable SRV007
    var client = fixture.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });
#pragma warning restore SRV007

    // Register the user with Identity API
    var registerRequest = new
    {
      email = email,
      password = password,
    };

    // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to interact with Identity API endpoints
    // which are not FastEndpoints. Identity API is provided by ASP.NET Core Identity (MapIdentityApi).
#pragma warning disable SRV007
    var registerResponse = await client.PostAsJsonAsync("/api/identity/register", registerRequest);
#pragma warning restore SRV007

    if (!registerResponse.IsSuccessStatusCode)
    {
      var errorContent = await registerResponse.Content.ReadAsStringAsync();
      throw new InvalidOperationException(
          $"Failed to register user. Status: {registerResponse.StatusCode}, Content: {errorContent}");
    }

    // Cookie is automatically stored in the client
    return (client, email, username);
  }

  /// <summary>
  /// Creates an authenticated HttpClient by logging in an existing user
  /// </summary>
  public static async Task<HttpClient> LoginUser<TFixture>(
      AppFixture<TFixture> fixture,
      string email,
      string password)
      where TFixture : class
  {
    // SRV007: Creating client with WebApplicationFactoryClientOptions for cookie support
#pragma warning disable SRV007
    var client = fixture.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });
#pragma warning restore SRV007

    var loginRequest = new
    {
      email = email,
      password = password,
    };

    // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to interact with Identity API endpoints
    // which are not FastEndpoints. Identity API is provided by ASP.NET Core Identity (MapIdentityApi).
#pragma warning disable SRV007
    var loginResponse = await client.PostAsJsonAsync("/api/identity/login", loginRequest);
#pragma warning restore SRV007

    if (!loginResponse.IsSuccessStatusCode)
    {
      var errorContent = await loginResponse.Content.ReadAsStringAsync();
      throw new InvalidOperationException(
          $"Failed to login user. Status: {loginResponse.StatusCode}, Content: {errorContent}");
    }

    return client;
  }
}
