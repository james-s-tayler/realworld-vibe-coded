namespace E2eTests.Tests.DashboardPage;

/// <summary>
/// Happy path tests for the Dashboard page (/).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task Dashboard_ShowsWelcomeMessage_WhenLoggedIn()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Assert
    await Expect(Pages.DashboardPage.WelcomeHeading).ToBeVisibleAsync();
    await Expect(Pages.DashboardPage.WelcomeHeading).ToContainTextAsync("Welcome");
  }

  [Fact]
  public async Task Dashboard_ShowsBanner_WhenFeatureFlagEnabled()
  {
    // Arrange — E2E runs with Development config where all flags are enabled
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Assert — banner should be visible because DashboardBanner flag is enabled
    await Expect(Pages.DashboardPage.FeatureBanner).ToBeVisibleAsync();
  }
}
