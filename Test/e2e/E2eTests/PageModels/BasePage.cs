using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Base class for all page models providing common functionality.
/// </summary>
/// <remarks>
/// This comment triggers Test/e2e change detection for flake detection jobs.
/// </remarks>
public abstract class BasePage
{
  protected const int DefaultTimeout = 10000;
  protected readonly IPage Page;
  protected readonly string BaseUrl;

  protected BasePage(IPage page, string baseUrl)
  {
    Page = page;
    BaseUrl = baseUrl;
  }

  /// <summary>
  /// Gets an assertion helper for this page.
  /// </summary>
  protected IPageAssertions Expect() => Assertions.Expect(Page);

  /// <summary>
  /// Gets an assertion helper for a locator.
  /// </summary>
  protected ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);

  /// <summary>
  /// Navigation header link - Conduit logo/home.
  /// </summary>
  public ILocator HomeLink => Page.GetByRole(AriaRole.Link, new() { Name = "conduit" });

  /// <summary>
  /// Navigation header link - Sign in.
  /// </summary>
  public ILocator SignInLink => Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" });

  /// <summary>
  /// Navigation header link - Sign up.
  /// </summary>
  public ILocator SignUpLink => Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" });

  /// <summary>
  /// Navigation header link - New Article.
  /// </summary>
  public ILocator NewArticleLink => Page.GetByRole(AriaRole.Link, new() { Name = "New Article" });

  /// <summary>
  /// Navigation header link - Settings.
  /// </summary>
  public ILocator SettingsLink => Page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true });

  /// <summary>
  /// Gets the user profile link in the navigation header.
  /// </summary>
  public ILocator GetUserProfileLink(string username) =>
    Page.GetByRole(AriaRole.Link, new() { Name = username }).First;

  /// <summary>
  /// Navigates to the home page.
  /// </summary>
  public async Task GoToHomePageAsync()
  {
    await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Clicks the Sign in link in the header.
  /// </summary>
  public async Task ClickSignInAsync()
  {
    await NavigateAndWaitForNetworkIdle(SignInLink, $"{BaseUrl}/login");
  }

  /// <summary>
  /// Clicks the Sign up link in the header.
  /// </summary>
  public async Task ClickSignUpAsync()
  {
    await NavigateAndWaitForNetworkIdle(SignUpLink, $"{BaseUrl}/register");
  }

  /// <summary>
  /// Clicks the New Article link in the header.
  /// </summary>
  public async Task ClickNewArticleAsync()
  {
    await NavigateAndWaitForNetworkIdle(NewArticleLink, $"{BaseUrl}/editor");
  }

  /// <summary>
  /// Clicks the Settings link in the header.
  /// </summary>
  public async Task ClickSettingsAsync()
  {
    await NavigateAndWaitForNetworkIdle(SettingsLink, $"{BaseUrl}/settings");
  }

  /// <summary>
  /// Clicks on the user's profile link in the header.
  /// </summary>
  public async Task ClickUserProfileAsync(string username)
  {
    await NavigateAndWaitForNetworkIdle(GetUserProfileLink(username), $"{BaseUrl}/profile/{username}");
  }

  /// <summary>
  /// Verifies that the user is logged in by checking for their username link.
  /// </summary>
  public async Task<bool> IsUserLoggedInAsync(string username)
  {
    try
    {
      await Expect(GetUserProfileLink(username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      return true;
    }
    catch
    {
      return false;
    }
  }

  /// <summary>
  /// Verifies that the Sign in link is visible (user is logged out).
  /// </summary>
  public async Task<bool> IsUserLoggedOutAsync()
  {
    try
    {
      await Expect(SignInLink).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      return true;
    }
    catch
    {
      return false;
    }
  }

  public virtual async Task GoToAsync(string urlParams = "")
  {
    if (!string.IsNullOrWhiteSpace(urlParams))
    {
      await Page.GotoAsync($"{BaseUrl}/{urlParams}");
    }
    else
    {
      await Page.GotoAsync(BaseUrl);
    }
  }

  private async Task NavigateAndWaitForNetworkIdle(ILocator locator, string url)
  {
    await locator.ClickAsync();
    await Expect().ToHaveURLAsync(url, new() { Timeout = DefaultTimeout });
  }
}
