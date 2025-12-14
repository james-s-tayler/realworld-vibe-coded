using Server.FunctionalTests.Users;

namespace Server.FunctionalTests.Identity;

/// <summary>
/// Tests for ASP.NET Core Identity API with cookie-based authentication.
/// Phase 4: Validates that Identity endpoints work and cookies can authenticate FastEndpoint requests.
/// </summary>
[Collection("Users Integration Tests")]
public class IdentityInfrastructureTests(UsersFixture app) : TestBase<UsersFixture>
{
  [Fact]
  public async Task IdentityRegister_WithValidCredentials_SetsAuthenticationCookie()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "TestPass123!";

    // Create client with cookie handling enabled using WebApplicationFactory directly
    // SRV007: Using Server.CreateDefaultClient for cookie support in Identity API testing
#pragma warning disable SRV007
    var client = app.Server.CreateDefaultClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });
#pragma warning restore SRV007

    var registerRequest = new { email, password };

    // SRV007: Using raw HttpClient.PostAsJsonAsync for Identity API endpoint (not a FastEndpoint)
#pragma warning disable SRV007
    var response = await client.PostAsJsonAsync("/api/identity/register", registerRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    // Verify cookie was set by checking response headers
    response.Headers.TryGetValues("Set-Cookie", out var cookies);
    cookies.ShouldNotBeNull();
    cookies.ShouldContain(c => c.Contains("Cookie"));
  }

  [Fact]
  public async Task IdentityLogin_ThenAccessFastEndpoint_UsesCookieForAuthentication()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "TestPass123!";

    // Create client with cookie handling enabled
    // SRV007: Using Server.CreateDefaultClient with cookie options for Identity API testing
#pragma warning disable SRV007
    var client = app.Server.CreateDefaultClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });

    // Register user via Identity API
    var registerRequest = new { email, password };

    var registerResponse = await client.PostAsJsonAsync("/api/identity/register", registerRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

    // Sync the Identity user to the old Users table so FastEndpoints can find it
    await SyncIdentityUserToUserTable(email, password);

    // Now try to access a FastEndpoint that requires authentication
    // The cookie from registration should authenticate this request
    var (response, _) = await client.GETAsync<Server.Web.Users.GetCurrent.GetCurrent, object>();

    // Should succeed because cookie authenticates the request
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task IdentityLogin_WithInvalidCredentials_ReturnsUnauthorized()
  {
    var loginRequest = new { email = "nonexistent@example.com", password = "wrong" };

    // SRV007: Using raw HttpClient.PostAsJsonAsync for Identity API endpoint
#pragma warning disable SRV007
    var response = await app.Client.PostAsJsonAsync("/api/identity/login", loginRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task IdentityRegister_WithWeakPassword_ReturnsBadRequest()
  {
    var registerRequest = new { email = $"test-{Guid.NewGuid()}@example.com", password = "weak" };

    // SRV007: Using raw HttpClient.PostAsJsonAsync for Identity API endpoint
#pragma warning disable SRV007
    var response = await app.Client.PostAsJsonAsync("/api/identity/register", registerRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  /// <summary>
  /// Helper to sync Identity user to old Users table for backward compatibility during migration
  /// </summary>
  private async Task SyncIdentityUserToUserTable(string email, string password)
  {
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Server.Core.IdentityAggregate.ApplicationUser>>();
    var syncService = scope.ServiceProvider.GetRequiredService<Server.Infrastructure.Services.UserIdentitySyncService>();

    var applicationUser = await userManager.FindByEmailAsync(email);
    if (applicationUser != null)
    {
      await syncService.SyncApplicationUserToUser(applicationUser, CancellationToken.None);
    }
  }
}
