using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Login page (/login).
/// </summary>
public class LoginPage : BasePage
{
  public LoginPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Email input field.
  /// </summary>
  public ILocator EmailInput => Page.GetByPlaceholder("Email");

  /// <summary>
  /// Password input field.
  /// </summary>
  public ILocator PasswordInput => Page.GetByPlaceholder("Password");

  /// <summary>
  /// Sign in button.
  /// </summary>
  public ILocator SignInButton => Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });

  /// <summary>
  /// Error display element.
  /// </summary>
  public ILocator ErrorDisplay => Page.GetByTestId("error-display");

  /// <summary>
  /// Fills in the login form.
  /// </summary>
  public async Task FillLoginFormAsync(string email, string password)
  {
    await EmailInput.FillAsync(email);
    await PasswordInput.FillAsync(password);
  }

  /// <summary>
  /// Clicks the sign in button.
  /// </summary>
  public async Task ClickSignInButtonAsync()
  {
    await SignInButton.ClickAsync();
    await Page.WaitForURLAsync(BaseUrl, new() { Timeout = DefaultTimeout });
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Performs a complete login action.
  /// </summary>
  /// <param name="email">User's email.</param>
  /// <param name="password">User's password.</param>
  /// <returns>HomePage after successful login.</returns>
  public async Task<HomePage> LoginAsync(string email, string password)
  {
    await FillLoginFormAsync(email, password);
    await ClickSignInButtonAsync();
    return new HomePage(Page, BaseUrl);
  }

  /// <summary>
  /// Attempts to login and expects it to fail.
  /// </summary>
  public async Task LoginAndExpectErrorAsync(string email, string password)
  {
    await FillLoginFormAsync(email, password);
    await ClickSignInButtonAsync();
    await Expect(ErrorDisplay).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
