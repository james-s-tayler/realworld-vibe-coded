using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Settings page (/settings).
/// </summary>
public class SettingsPage : BasePage
{
  public SettingsPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Profile image URL input field.
  /// </summary>
  public ILocator ImageUrlInput => Page.GetByPlaceholder("URL of profile picture");

  /// <summary>
  /// Username input field.
  /// </summary>
  public ILocator UsernameInput => Page.GetByPlaceholder("Username");

  /// <summary>
  /// Bio textarea.
  /// </summary>
  public ILocator BioInput => Page.GetByPlaceholder("Short bio about you");

  /// <summary>
  /// Email input field.
  /// </summary>
  public ILocator EmailInput => Page.GetByPlaceholder("Email");

  /// <summary>
  /// Password input field.
  /// </summary>
  public ILocator PasswordInput => Page.GetByPlaceholder("New Password");

  /// <summary>
  /// Update settings button.
  /// </summary>
  public ILocator UpdateSettingsButton => Page.GetByRole(AriaRole.Button, new() { Name = "Update Settings" });

  /// <summary>
  /// Logout button.
  /// </summary>
  public ILocator LogoutButton => Page.GetByRole(AriaRole.Button, new() { Name = "Or click here to logout." });

  /// <summary>
  /// Success message after updating settings.
  /// </summary>
  public ILocator SuccessMessage => Page.GetByText("Settings updated successfully");

  /// <summary>
  /// Waits for the bio input to be visible (settings page loaded).
  /// </summary>
  public async Task WaitForPageToLoadAsync()
  {
    await BioInput.WaitForAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Updates the bio field.
  /// </summary>
  public async Task UpdateBioAsync(string bio)
  {
    await BioInput.FillAsync(bio);
  }

  /// <summary>
  /// Clicks the update settings button.
  /// </summary>
  public async Task ClickUpdateSettingsButtonAsync()
  {
    await UpdateSettingsButton.ClickAsync();
  }

  /// <summary>
  /// Updates the bio and saves the settings.
  /// </summary>
  public async Task UpdateAndSaveBioAsync(string bio)
  {
    await WaitForPageToLoadAsync();
    await UpdateBioAsync(bio);
    await ClickUpdateSettingsButtonAsync();
    await Expect(SuccessMessage).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Clicks the logout button.
  /// </summary>
  public async Task ClickLogoutButtonAsync()
  {
    await LogoutButton.ClickAsync();
  }

  /// <summary>
  /// Performs logout and verifies the user is logged out.
  /// </summary>
  public async Task LogoutAsync()
  {
    await ClickLogoutButtonAsync();
    await Expect(SignInLink).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the success message is displayed.
  /// </summary>
  public async Task VerifySuccessMessageAsync()
  {
    await Expect(SuccessMessage).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the Sign in link is visible (user is logged out).
  /// </summary>
  public async Task VerifyLoggedOutAsync()
  {
    await Expect(SignInLink).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    Assert.True(await SignInLink.IsVisibleAsync(), "Sign in link should be visible after logout");

    Assert.True(await SignUpLink.IsVisibleAsync(), "Sign up link should be visible after logout");
  }
}
