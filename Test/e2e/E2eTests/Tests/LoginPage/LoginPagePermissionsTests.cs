using Microsoft.Playwright;

namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Permission tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class LoginPagePermissionsTests : ConduitPageTest
{
  [Fact]
  public async Task ProtectedRoutes_RedirectToLogin_WhenNotAuthenticated()
  {
    // Try to access editor page without authentication
    var editorPage = GetEditorPage();
    await editorPage.GoToAsync();
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = DefaultTimeout });

    // Should redirect to login
    await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"), new() { Timeout = DefaultTimeout });

    // Try to access settings page without authentication
    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();

    // Should redirect to login
    await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"), new() { Timeout = DefaultTimeout });
  }
}
