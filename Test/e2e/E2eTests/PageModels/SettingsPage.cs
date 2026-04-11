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
  /// Success message after updating settings.
  /// </summary>
  public ILocator SuccessMessage => Page.GetByText("Settings updated successfully");

  /// <summary>
  /// Error display element.
  /// </summary>
  public ILocator ErrorDisplay => Page.GetByTestId("toast-error");

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
    await UpdateBioAsync(bio);
    await ClickUpdateSettingsButtonAsync();
    await Expect(SuccessMessage).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the success message is displayed.
  /// </summary>
  public async Task VerifySuccessMessageAsync()
  {
    await Expect(SuccessMessage).ToBeVisibleAsync();
  }

  /// <summary>
  /// Language dropdown.
  /// </summary>
  public ILocator LanguageDropdown => Page.Locator("#language");

  /// <summary>
  /// Selects a language from the dropdown.
  /// </summary>
  public async Task ChangeLanguageAsync(string languageLabel)
  {
    await LanguageDropdown.ClickAsync();
    await Page.GetByText(languageLabel).ClickAsync();
  }

  /// <summary>
  /// Attempts to update settings and expects it to fail with an error.
  /// </summary>
  public async Task UpdateSettingsAndExpectErrorAsync(string? username = null, string? email = null)
  {
    if (username != null)
    {
      await UsernameInput.ClearAsync();
      await UsernameInput.FillAsync(username);
    }

    if (email != null)
    {
      await EmailInput.ClearAsync();
      await EmailInput.FillAsync(email);
    }

    await ClickUpdateSettingsButtonAsync();
    await Expect(ErrorDisplay).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the error display contains specific text.
  /// </summary>
  public async Task VerifyErrorContainsTextAsync(string expectedText)
  {
    await Expect(ErrorDisplay).ToContainTextAsync(expectedText);
  }
}
