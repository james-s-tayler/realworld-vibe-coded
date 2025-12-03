namespace E2eTests.Tests;

/// <summary>
/// Tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class LoginPageTests : ConduitPageTest
{
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
      await RegisterUserAsync();

      // Sign out
      await SignOutAsync();

      // Now sign in with the credentials using page model
      var loginPage = GetLoginPage();
      await loginPage.GoToAsync();
      var homePage = await loginPage.LoginAsync(TestEmail, TestPassword);

      // Verify user is logged in
      Assert.True(await homePage.IsUserLoggedInAsync(TestUsername), "User should be logged in after sign in");
    }
    finally
    {
      await SaveTrace("sign_in_test");
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
      var editorPage = GetEditorPage();
      await editorPage.GoToAsync();

      // Should redirect to login
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = 10000 });
      Assert.Contains("/login", Page.Url);

      // Try to access settings page without authentication
      var settingsPage = GetSettingsPage();
      await settingsPage.GoToAsync();

      // Should redirect to login
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = 10000 });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("protected_routes_test");
    }
  }
}
