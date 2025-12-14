using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.Infrastructure.Services;

namespace Server.FunctionalTests;

/// <summary>
/// Helper class for creating authenticated HttpClients using ASP.NET Core Identity endpoints
/// </summary>
public static class IdentityAuthHelper
{
  /// <summary>
  /// Creates an authenticated HttpClient by registering a new user with Identity and capturing the authentication cookie.
  /// Also syncs the ApplicationUser to the old Users table for backward compatibility.
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

    // Sync ApplicationUser to User table for backward compatibility
    await SyncIdentityUserToOldUserTable(fixture, email, password);

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

  /// <summary>
  /// Syncs an Identity user to the old Users table by calling the sync service
  /// </summary>
  private static async Task SyncIdentityUserToOldUserTable<TFixture>(
      AppFixture<TFixture> fixture,
      string email,
      string password)
      where TFixture : class
  {
    // Get services from the test fixture
    using var scope = fixture.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var syncService = scope.ServiceProvider.GetRequiredService<UserIdentitySyncService>();

    // Find the ApplicationUser that was just created
    var applicationUser = await userManager.FindByEmailAsync(email);
    if (applicationUser == null)
    {
      throw new InvalidOperationException($"ApplicationUser not found for email {email}");
    }

    // Sync to old Users table
    await syncService.SyncApplicationUserToUser(applicationUser, CancellationToken.None);
  }
}
