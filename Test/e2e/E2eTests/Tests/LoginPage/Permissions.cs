namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Permission tests for the Login page (/login).
/// </summary>
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "auth-permissions-001",
    FeatureArea = "auth",
    Behavior = "Unauthenticated user accessing editor is redirected to login",
    Verifies = ["URL changes to /login"])]
  public async Task EditorPage_RedirectToLogin_WhenNotAuthenticated()
  {
    // Arrange
    await Pages.EditorPage.GoToAsync();

    // Act + Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  [TestCoverage(
    Id = "auth-permissions-002",
    FeatureArea = "auth",
    Behavior = "Unauthenticated user accessing settings is redirected to login",
    Verifies = ["URL changes to /login"])]
  public async Task SettingsPage_RedirectToLogin_WhenNotAuthenticated()
  {
    // Arrange
    await Pages.SettingsPage.GoToAsync();

    // Act + Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
