namespace E2eTests.Tests.Mobile;

public class Screenshots : MobileAppPageTest
{
  public Screenshots(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task LoginPage()
  {
    await Pages.LoginPage.GoToAsync();

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  public async Task RegisterPage()
  {
    await Pages.RegisterPage.GoToAsync();

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  public async Task SettingsPage()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);
    await Page.GotoAsync($"{BaseUrl}/settings");

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  public async Task DashboardPage()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);
    await Page.GotoAsync($"{BaseUrl}/");

    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
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
