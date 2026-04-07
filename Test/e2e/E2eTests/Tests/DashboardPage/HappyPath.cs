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
  [TestCoverage(
    Id = "dashboard-happy-001",
    FeatureArea = "dashboard",
    Behavior = "Dashboard shows welcome message when user is logged in",
    Verifies = ["Welcome heading is visible", "Welcome heading contains 'Welcome'"])]
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
  [TestCoverage(
    Id = "dashboard-happy-002",
    FeatureArea = "dashboard",
    Behavior = "Dashboard shows feature banner when DashboardBanner flag is enabled",
    Verifies = ["Feature banner is visible"])]
  public async Task Dashboard_ShowsBanner_WhenFeatureFlagEnabled()
  {
    // Arrange — E2E runs with Development config where all flags are enabled
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Assert — banner should be visible because DashboardBanner flag is enabled
    await Expect(Pages.DashboardPage.FeatureBanner).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "dashboard-happy-003",
    FeatureArea = "dashboard",
    Behavior = "Dashboard banner disappears when feature flag is toggled off and reappears when toggled on",
    Verifies = ["Banner visible initially", "Banner disappears after flag disabled", "Banner reappears after flag re-enabled"])]
  public async Task Dashboard_BannerDisappears_WhenFeatureFlagToggledOff()
  {
    // Arrange — login and verify banner is visible
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Expect(Pages.DashboardPage.FeatureBanner).ToBeVisibleAsync();

    try
    {
      // Act — disable the feature flag via DevOnly endpoint
      await Api.SetFeatureFlagOverrideAsync("DashboardBanner", false);

      // Assert — banner should disappear after frontend refreshes (Development = 3s interval)
      await Expect(Pages.DashboardPage.FeatureBanner).Not.ToBeVisibleAsync(new() { Timeout = 10_000 });

      // Act — re-enable the feature flag
      await Api.SetFeatureFlagOverrideAsync("DashboardBanner", true);

      // Assert — banner should reappear after frontend refreshes
      await Expect(Pages.DashboardPage.FeatureBanner).ToBeVisibleAsync(new() { Timeout = 10_000 });
    }
    finally
    {
      // Always re-enable the flag to prevent state leakage to other tests
      await Api.SetFeatureFlagOverrideAsync("DashboardBanner", true);
    }
  }
}
