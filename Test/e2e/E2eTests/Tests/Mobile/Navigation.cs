using Microsoft.Playwright;

namespace E2eTests.Tests.Mobile;

public class Navigation : MobileAppPageTest
{
  public Navigation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task SidebarIsHiddenOnPageLoad()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await Expect(SideNav).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task HamburgerButtonIsVisibleOnMobile()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await Expect(HamburgerButton).ToBeVisibleAsync();
  }

  [Fact]
  public async Task HamburgerToggleOpensSidebar()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await HamburgerButton.ClickAsync();

    await Expect(SideNav).ToBeVisibleAsync();
  }

  [Fact]
  public async Task NavLinkNavigatesAndClosesSidebar()
  {
    var user = await Api.CreateUserAsync();
    await LoginOnMobileAsync(user.Email, user.Password);

    await HamburgerButton.ClickAsync();

    // Wait for the Settings link to be in viewport (SideNav slide-in animation must complete),
    // then dispatch click via DOM event. Carbon's SideNav overlay on mobile renders inside the
    // Header's stacking context, which can cause page content to intercept pointer events.
    var settingsLink = SideNav.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true });
    await Expect(settingsLink).ToBeInViewportAsync();
    await settingsLink.DispatchEventAsync("click");

    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/settings");
    await Expect(SideNav).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task UnauthenticatedUserSeesSignInSignUp()
  {
    await Pages.LoginPage.GoToAsync();

    await HamburgerButton.ClickAsync();

    await Expect(SideNav.GetByRole(AriaRole.Link, new() { Name = "Sign in" })).ToBeVisibleAsync();
    await Expect(SideNav.GetByRole(AriaRole.Link, new() { Name = "Sign up" })).ToBeVisibleAsync();
  }
}
