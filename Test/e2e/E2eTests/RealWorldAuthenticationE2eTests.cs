using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldAuthenticationE2eTests : PageTest
{
  private const int DefaultTimeout = 10000;
  private string _baseUrl = null!;
  private string _testUsername = null!;
  private string _testEmail = null!;
  private string _testPassword = null!;

  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true,
    };
  }

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    var timestamp = DateTime.Now.Ticks;
    _testUsername = $"authuser{timestamp}";
    _testEmail = $"authuser{timestamp}@test.com";
    _testPassword = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign In Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, register a user
      await RegisterUser();

      // Sign out
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/settings", new() { Timeout = DefaultTimeout });
      await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
      await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });

      // Now sign in with the credentials
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/login", new() { Timeout = DefaultTimeout });

      await Page.GetByPlaceholder("Email").FillAsync(_testEmail);
      await Page.GetByPlaceholder("Password").FillAsync(_testPassword);
      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

      // Verify redirect to home page
      await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });

      // Verify user is logged in
      var userLink = Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First;
      await userLink.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await userLink.IsVisibleAsync(), "User should be logged in after sign in");
    }
    finally
    {
      await SaveTrace("sign_in_test");
    }
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign Out Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register and login a user
      await RegisterUser();

      // Navigate to settings
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/settings", new() { Timeout = DefaultTimeout });

      // Click logout button
      await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

      // Verify redirect to home page
      await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });

      // Verify user is logged out - Sign in and Sign up links should be visible
      var signInLink = Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" });
      await signInLink.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await signInLink.IsVisibleAsync(), "Sign in link should be visible after logout");

      var signUpLink = Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" });
      Assert.True(await signUpLink.IsVisibleAsync(), "Sign up link should be visible after logout");
    }
    finally
    {
      await SaveTrace("sign_out_test");
    }
  }

  [Fact]
  public async Task ProtectedRoutes_RedirectToLogin_WhenNotAuthenticated()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Protected Routes Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Try to access editor page without authentication
      await Page.GotoAsync($"{_baseUrl}/editor", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Should redirect to login
      await Page.WaitForURLAsync($"{_baseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);

      // Try to access settings page without authentication
      await Page.GotoAsync($"{_baseUrl}/settings", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Should redirect to login
      await Page.WaitForURLAsync($"{_baseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("protected_routes_test");
    }
  }

  // Helper methods
  private async Task RegisterUser()
  {
    await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/register", new() { Timeout = DefaultTimeout });

    await Page.GetByPlaceholder("Username").FillAsync(_testUsername);
    await Page.GetByPlaceholder("Email").FillAsync(_testEmail);
    await Page.GetByPlaceholder("Password").FillAsync(_testPassword);

    // Click submit and wait for API response and navigation
    var responseTask = Page.WaitForResponseAsync(
      response =>
      response.Url.Contains("/api/users") && response.Request.Method == "POST",
      new() { Timeout = DefaultTimeout });

    await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

    await responseTask;

    // Wait for the user link to appear in the header to confirm login and navigation completed
    await Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
  }

  private async Task SaveTrace(string testName)
  {
    if (!Directory.Exists(Constants.TracesDirectory))
    {
      Directory.CreateDirectory(Constants.TracesDirectory);
    }

    await Context.Tracing.StopAsync(new()
    {
      Path = Path.Combine(Constants.TracesDirectory, $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
    });
  }
}
