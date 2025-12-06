namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Permission tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
{
  [Fact]
  public async Task ProtectedRoutes_RedirectToLogin_WhenNotAuthenticated()
  {
    // Arrange
    await Pages.EditorPage.GoToAsync();

    // Act + Assert
    await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"), new() { Timeout = DefaultTimeout });

    // Arrange
    await Pages.SettingsPage.GoToAsync();

    // Act + Assert
    await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"), new() { Timeout = DefaultTimeout });
  }
}
