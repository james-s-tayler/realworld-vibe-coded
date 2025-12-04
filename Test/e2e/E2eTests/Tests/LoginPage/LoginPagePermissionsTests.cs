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
}
