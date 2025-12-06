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
  }

  /// <summary>
  /// Performs a complete login action and waits for navigation to home page.
  /// </summary>
  /// <param name="email">User's email.</param>
  /// <param name="password">User's password.</param>
  public async Task LoginAsync(string email, string password)
  {
    await FillLoginFormAsync(email, password);
    await ClickSignInButtonAsync();

    // Wait for successful login by verifying the New Article link is visible (only shown when logged in)
    await Expect(NewArticleLink).ToBeVisibleAsync();
  }
}
