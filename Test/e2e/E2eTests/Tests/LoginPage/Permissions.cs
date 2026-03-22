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
  public async Task SettingsPage_RedirectToLogin_WhenNotAuthenticated()
  {
    // Arrange
    await Pages.SettingsPage.GoToAsync();

    // Act + Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
