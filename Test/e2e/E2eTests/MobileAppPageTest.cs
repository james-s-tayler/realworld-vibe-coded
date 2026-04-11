using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests;

public abstract class MobileAppPageTest : AppPageTest
{
  protected MobileAppPageTest(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true,
      ViewportSize = new ViewportSize { Width = 375, Height = 667 },
      IsMobile = true,
      HasTouch = true,
    };
  }

  protected ILocator HamburgerButton =>
    Page.GetByRole(AriaRole.Button, new() { Name = "Open menu" });

  protected ILocator CloseMenuButton =>
    Page.GetByRole(AriaRole.Button, new() { Name = "Close menu" });

  protected ILocator SideNav =>
    Page.GetByRole(AriaRole.Navigation, new() { Name = "Side navigation" });

  /// <summary>
  /// Login on mobile by filling the form and submitting.
  /// Cannot use LoginPage.LoginAsync() because it asserts the Settings link
  /// is visible in the sidebar, which is hidden on mobile.
  /// Waits for URL to leave /login to confirm navigation completed —
  /// the hamburger button alone is not a reliable signal because it is
  /// always visible on mobile regardless of auth state.
  /// </summary>
  protected async Task LoginOnMobileAsync(string email, string password)
  {
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.FillLoginFormAsync(email, password);
    await Pages.LoginPage.ClickSignInButtonAsync();

    // Wait for navigation away from login page to confirm login succeeded
    await Expect(Page).Not.ToHaveURLAsync(new Regex("/login"));
    await Expect(HamburgerButton).ToBeVisibleAsync();
  }
}
