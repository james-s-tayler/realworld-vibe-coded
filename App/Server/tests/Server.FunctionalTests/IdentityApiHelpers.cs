using System.Net.Http.Headers;

namespace Server.FunctionalTests;

/// <summary>
/// Helper methods for interacting with Identity API endpoints
/// </summary>
public static class IdentityApiHelpers
{
  /// <summary>
  /// Register a new user via Identity API
  /// </summary>
  public static async Task<string> RegisterUserAsync(
    HttpClient client,
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var registerPayload = new
    {
      email,
      password,
    };

    var response = await client.PostAsJsonAsync(
      "/api/identity/register",
      registerPayload,
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IdentityRegisterResponse>(cancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Registration did not return an access token");
  }

  /// <summary>
  /// Register a user and return an authenticated HttpClient
  /// </summary>
  public static async Task<(HttpClient Client, string Email, string AccessToken)> RegisterUserAndCreateClientAsync(
    HttpClient baseClient,
    Func<Action<HttpClient>, HttpClient> createClientFunc,
    string? email = null,
    string? password = null,
    CancellationToken cancellationToken = default)
  {
    email ??= $"user-{Guid.NewGuid()}@example.com";
    password ??= "Password123!";

    var accessToken = await RegisterUserAsync(baseClient, email, password, cancellationToken);
    var client = CreateAuthenticatedClient(createClientFunc, accessToken);

    return (client, email, accessToken);
  }

  /// <summary>
  /// Login a user via Identity API
  /// </summary>
  public static async Task<string> LoginUserAsync(
    HttpClient client,
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var loginPayload = new
    {
      email,
      password,
    };

    var response = await client.PostAsJsonAsync(
      "/api/identity/login?useCookies=false",
      loginPayload,
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IdentityLoginResponse>(cancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
  }

  /// <summary>
  /// Create an authenticated HttpClient with Bearer token
  /// </summary>
  public static HttpClient CreateAuthenticatedClient(
    Func<Action<HttpClient>, HttpClient> createClientFunc,
    string accessToken)
  {
    return createClientFunc(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    });
  }

  private record IdentityRegisterResponse(string AccessToken);

  private record IdentityLoginResponse(string AccessToken);
}
