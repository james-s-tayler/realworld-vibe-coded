using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Profile page (/profile/:username).
/// </summary>
public class ProfilePage : BasePage
{
  public ProfilePage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Profile username heading.
  /// </summary>
  public ILocator GetUsernameHeading(string username) =>
    Page.GetByRole(AriaRole.Heading, new() { Name = username, Exact = true });

  /// <summary>
  /// Bio text on the profile.
  /// </summary>
  public ILocator GetBioText(string bioText) => Page.GetByText(bioText);

  /// <summary>
  /// Edit Profile Settings button.
  /// </summary>
  public ILocator EditProfileSettingsButton =>
    Page.GetByRole(AriaRole.Button, new() { Name = "Edit Profile Settings" });

  /// <summary>
  /// Waits for the profile to load.
  /// </summary>
  public async Task WaitForProfileToLoadAsync(string username)
  {
    var heading = GetUsernameHeading(username);
    await Expect(heading).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the profile heading is visible.
  /// </summary>
  public async Task VerifyProfileHeadingAsync(string username)
  {
    var heading = GetUsernameHeading(username);
    await Expect(heading).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the bio text is visible on the profile.
  /// </summary>
  public async Task VerifyBioVisibleAsync(string bioText)
  {
    await Expect(GetBioText(bioText)).ToBeVisibleAsync();
  }
}
