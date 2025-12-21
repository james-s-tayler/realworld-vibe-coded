using System.Net.Http.Headers;

namespace Server.FunctionalTests;

/// <summary>
/// Base fixture class that provides Identity API helper methods for test fixtures
/// </summary>
public abstract class ApiFixtureBase<TProgram> : AppFixture<TProgram>
  where TProgram : class
{
  public async Task<string> RegisterUserAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var registerPayload = new
    {
      email,
      password,
    };

    var response = await Client.PostAsJsonAsync(
      "/api/identity/register",
      registerPayload,
      cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
      throw new InvalidOperationException($"Registration failed with status {response.StatusCode}. Response: {errorContent}");
    }

    var result = await response.Content.ReadFromJsonAsync<IdentityRegisterResponse>(cancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Registration did not return an access token");
  }

  public async Task<(HttpClient Client, string Email, string AccessToken)> RegisterUserAndCreateClientAsync(
    string? email = null,
    string? password = null,
    CancellationToken cancellationToken = default)
  {
    email ??= $"user-{Guid.NewGuid()}@example.com";
    password ??= "Password123!";

    var accessToken = await RegisterUserAsync(email, password, cancellationToken);
    var client = CreateAuthenticatedClient(accessToken);

    return (client, email, accessToken);
  }

  public async Task<string> LoginUserAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var loginPayload = new
    {
      email,
      password,
    };

    var response = await Client.PostAsJsonAsync(
      "/api/identity/login?useCookies=false",
      loginPayload,
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IdentityLoginResponse>(cancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
  }

  public HttpClient CreateAuthenticatedClient(string accessToken)
  {
    return CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    });
  }

  private record IdentityRegisterResponse(string AccessToken);

  private record IdentityLoginResponse(string AccessToken);
}
