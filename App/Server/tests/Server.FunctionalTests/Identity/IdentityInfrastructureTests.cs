using System.Text.Json;
using Server.FunctionalTests.Users;

namespace Server.FunctionalTests.Identity;

/// <summary>
/// Tests for ASP.NET Core Identity API endpoints - verifies Identity infrastructure is wired up correctly
/// These tests validate that Identity endpoints are callable and cookie authentication works,
/// but do not test integration with application business logic (which still uses old User table).
/// </summary>
[Collection("Users Integration Tests")]
public class IdentityInfrastructureTests(UsersFixture app) : TestBase<UsersFixture>
{
  [Fact]
  public async Task IdentityRegister_WithValidCredentials_ReturnsOkAndSetsCookie()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var password = "TestPass123!";

    var (client, returnedEmail, _) = await IdentityAuthHelper.CreateAuthenticatedClient(app, email, password);

    returnedEmail.ShouldBe(email);

    // Verify we can make an authenticated request with the cookie
    // SRV007: Using raw HttpClient.GetAsync is necessary to test Identity's /manage/info endpoint
    // which is not a FastEndpoint. This is infrastructure testing for Identity API.
#pragma warning disable SRV007
    var response = await client.GetAsync("/api/identity/manage/info", TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var json = JsonDocument.Parse(content);
    var infoEmail = json.RootElement.GetProperty("email").GetString();

    infoEmail.ShouldBe(email);
  }

  [Fact]
  public async Task IdentityRegister_WithDuplicateEmail_ReturnsBadRequest()
  {
    var email = $"duplicate-{Guid.NewGuid()}@example.com";
    var password = "TestPass123!";

    // First registration should succeed
    await IdentityAuthHelper.CreateAuthenticatedClient(app, email, password);

    // Second registration with same email should fail
    // SRV007: CreateClient with options and PostAsJsonAsync needed for Identity API testing
#pragma warning disable SRV007
    var client = app.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });

    var registerRequest = new
    {
      email = email,
      password = password,
    };

    var response = await client.PostAsJsonAsync("/api/identity/register", registerRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task IdentityLogin_WithValidCredentials_ReturnsOkAndSetsCookie()
  {
    var email = $"login-{Guid.NewGuid()}@example.com";
    var password = "TestPass123!";

    // Register first
    await IdentityAuthHelper.CreateAuthenticatedClient(app, email, password);

    // Login with a new client
    // SRV007: LoginUser internally uses raw HttpClient methods for Identity API testing
#pragma warning disable SRV007
    var loginClient = await IdentityAuthHelper.LoginUser(app, email, password);
#pragma warning restore SRV007

    // Verify cookie works
    // SRV007: Using raw HttpClient.GetAsync is necessary to test Identity's /manage/info endpoint
    // which is not a FastEndpoint. This is infrastructure testing for Identity API.
#pragma warning disable SRV007
    var response = await loginClient.GetAsync("/api/identity/manage/info", TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task IdentityLogin_WithInvalidCredentials_ReturnsUnauthorized()
  {
    // SRV007: CreateClient with options and PostAsJsonAsync needed for Identity API testing
#pragma warning disable SRV007
    var client = app.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });

    var loginRequest = new
    {
      email = "nonexistent@example.com",
      password = "wrongpassword",
    };

    var response = await client.PostAsJsonAsync("/api/identity/login", loginRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task IdentityManageInfo_WithoutCookie_ReturnsUnauthorized()
  {
    // SRV007: CreateClient and GetAsync needed for Identity API testing
#pragma warning disable SRV007
    var client = app.CreateClient();

    var response = await client.GetAsync("/api/identity/manage/info", TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task IdentityRegister_WithWeakPassword_ReturnsBadRequest()
  {
    // SRV007: CreateClient with options and PostAsJsonAsync needed for Identity API testing
#pragma warning disable SRV007
    var client = app.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false,
      HandleCookies = true,
    });

    var registerRequest = new
    {
      email = $"test-{Guid.NewGuid()}@example.com",
      password = "weak",
    };

    var response = await client.PostAsJsonAsync("/api/identity/register", registerRequest, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CookieAuth_WorksWithFastEndpoints()
  {
    // This test verifies that cookie authentication works with FastEndpoints
    // that have been configured to accept IdentityConstants.ApplicationScheme

    var (client, _, _) = await IdentityAuthHelper.CreateAuthenticatedClient(app);

    // Try to access a FastEndpoint that accepts cookie auth (we updated them earlier)
    // Use the GetCurrent endpoint as an example - it should fail because the ApplicationUser
    // is not synced with the User table yet, but we're just testing that auth schema works
    var (response, _) = await client.GETAsync<Server.Web.Users.GetCurrent.GetCurrent, object>();

    // We expect either 200 (if user somehow exists) or 404 (user not in old User table)
    // The key is that we should NOT get 401 Unauthorized, which would indicate auth schema issue
    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
  }
}
