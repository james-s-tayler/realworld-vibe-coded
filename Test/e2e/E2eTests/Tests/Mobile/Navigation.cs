using Microsoft.Playwright;

namespace E2eTests.Tests.Mobile;

public class Navigation : MobileAppPageTest
{
  public Navigation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-happy-001",
    FeatureArea = "mobile",
    Behavior = "Sidebar is hidden by default on mobile viewport",
    Verifies = ["SideNav is not visible"])]
  public async Task SidebarIsHiddenOnPageLoad()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await Expect(SideNav).Not.ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-happy-002",
    FeatureArea = "mobile",
    Behavior = "Hamburger menu button is visible on mobile viewport",
    Verifies = ["Hamburger button is visible"])]
  public async Task HamburgerButtonIsVisibleOnMobile()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await Expect(HamburgerButton).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-happy-003",
    FeatureArea = "mobile",
    Behavior = "Tapping hamburger button opens the sidebar",
    Verifies = ["SideNav becomes visible"])]
  public async Task HamburgerToggleOpensSidebar()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await HamburgerButton.DispatchEventAsync("click");

    await Expect(SideNav).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-happy-004",
    FeatureArea = "mobile",
    Behavior = "Tapping a nav link navigates to the page and closes the sidebar",
    Verifies = ["URL changes to /settings", "SideNav closes after navigation"])]
  public async Task NavLinkNavigatesAndClosesSidebar()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await HamburgerButton.ClickAsync();

    // Wait for SideNav to be visible (confirms state change from hamburger click),
    // then dispatch click via DOM event. Carbon's SideNav overlay on mobile renders inside the
    // Header's stacking context, which can cause page content to intercept pointer events.
    // DispatchEventAsync bypasses hit-testing entirely and doesn't require viewport intersection.
    await Expect(SideNav).ToBeVisibleAsync();
    var settingsLink = SideNav.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true });
    await settingsLink.DispatchEventAsync("click");

    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/settings");
    await Expect(SideNav).Not.ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "mobile-happy-005",
    FeatureArea = "mobile",
    Behavior = "Unauthenticated user sees Sign in and Sign up links in mobile sidebar",
    Verifies = ["'Sign in' link visible in SideNav", "'Sign up' link visible in SideNav"])]
  public async Task UnauthenticatedUserSeesSignInSignUp()
  {
    await Pages.LoginPage.GoToAsync();

    await HamburgerButton.DispatchEventAsync("click");

    await Expect(SideNav.GetByRole(AriaRole.Link, new() { Name = "Sign in" })).ToBeVisibleAsync();
    await Expect(SideNav.GetByRole(AriaRole.Link, new() { Name = "Sign up" })).ToBeVisibleAsync();
  }
}
