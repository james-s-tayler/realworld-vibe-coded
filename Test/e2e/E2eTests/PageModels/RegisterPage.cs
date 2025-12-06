using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Registration page (/register).
/// </summary>
public class RegisterPage : BasePage
{
  public RegisterPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Username input field.
  /// </summary>
  public ILocator UsernameInput => Page.GetByPlaceholder("Username");

  /// <summary>
  /// Email input field.
  /// </summary>
  public ILocator EmailInput => Page.GetByPlaceholder("Email");

  /// <summary>
  /// Password input field.
  /// </summary>
  public ILocator PasswordInput => Page.GetByPlaceholder("Password");

  /// <summary>
  /// Sign up button.
  /// </summary>
  public ILocator SignUpButton => Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" });

  /// <summary>
  /// Error display element.
  /// </summary>
  public ILocator ErrorDisplay => Page.GetByTestId("error-display");

  /// <summary>
  /// Fills in the registration form.
  /// </summary>
  public async Task FillRegistrationFormAsync(string username, string email, string password)
  {
    await UsernameInput.FillAsync(username);
    await EmailInput.FillAsync(email);
    await PasswordInput.FillAsync(password);
  }

  /// <summary>
  /// Clicks the sign up button.
  /// </summary>
  public async Task ClickSignUpButtonAsync()
  {
    await SignUpButton.ClickAsync();
  }

  /// <summary>
  /// Performs a complete registration action and waits for the API response.
  /// </summary>
  /// <param name="username">Username for the new account.</param>
  /// <param name="email">Email for the new account.</param>
  /// <param name="password">Password for the new account.</param>
  /// <returns>HomePage after successful registration.</returns>
  public async Task<HomePage> RegisterAsync(string username, string email, string password)
  {
    await FillRegistrationFormAsync(username, email, password);
    await ClickSignUpButtonAsync();
    await Expect(GetUserProfileLink(username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

    return new HomePage(Page, BaseUrl);
  }

  /// <summary>
  /// Attempts to register and expects it to fail with an error.
  /// </summary>
  public async Task RegisterAndExpectErrorAsync(string username, string email, string password)
  {
    await FillRegistrationFormAsync(username, email, password);
    await ClickSignUpButtonAsync();
    await Expect(ErrorDisplay).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the error display contains specific text.
  /// </summary>
  public async Task VerifyErrorContainsTextAsync(string expectedText)
  {
    await Expect(ErrorDisplay).ToContainTextAsync(expectedText, new() { Timeout = DefaultTimeout });
  }
}
