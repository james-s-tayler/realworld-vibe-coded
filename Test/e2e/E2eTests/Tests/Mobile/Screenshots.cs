namespace E2eTests.Tests.Mobile;

public class Screenshots : MobileAppPageTest
{
  public Screenshots(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-screenshots-001",
    FeatureArea = "mobile",
    Behavior = "Login page renders correctly on mobile with no horizontal overflow",
    Verifies = ["Screenshot width within viewport"])]
  public async Task LoginPage()
  {
    await Pages.LoginPage.GoToAsync();

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-screenshots-002",
    FeatureArea = "mobile",
    Behavior = "Register page renders correctly on mobile with no horizontal overflow",
    Verifies = ["Screenshot width within viewport"])]
  public async Task RegisterPage()
  {
    await Pages.RegisterPage.GoToAsync();

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-screenshots-003",
    FeatureArea = "mobile",
    Behavior = "Settings page renders correctly on mobile with no horizontal overflow",
    Verifies = ["Screenshot width within viewport"])]
  public async Task SettingsPage()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);
    await Page.GotoAsync($"{BaseUrl}/settings");

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-screenshots-004",
    FeatureArea = "mobile",
    Behavior = "Dashboard page renders correctly on mobile with no horizontal overflow",
    Verifies = ["Screenshot width within viewport"])]
  public async Task DashboardPage()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);
    await Page.GotoAsync($"{BaseUrl}/");

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-screenshots-005",
    FeatureArea = "mobile",
    Behavior = "Sidebar open state renders correctly on mobile with no horizontal overflow",
    Verifies = ["Screenshot width within viewport"])]
  public async Task SidebarOpenOnMobile()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await HamburgerButton.ClickAsync();
    await Expect(SideNav).ToBeVisibleAsync();

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }
}
