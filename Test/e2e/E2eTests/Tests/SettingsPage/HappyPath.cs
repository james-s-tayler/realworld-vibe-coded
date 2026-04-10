using Microsoft.Playwright;

namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Happy path tests for the Settings page (/settings).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanEditProfile()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    var bioText = "This is my updated bio for E2E test";

    // Act
    await Pages.SettingsPage.UpdateAndSaveBioAsync(bioText);

    // Assert
    await Pages.ProfilePage.GoToAsync(user.Email);
    await Pages.ProfilePage.WaitForProfileToLoadAsync(user.Email);

    await Pages.ProfilePage.VerifyBioVisibleAsync(bioText);
  }

  [Fact]
  public async Task UserCanChangeLanguageToJapanese()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act — change language to Japanese
    await Pages.SettingsPage.ChangeLanguageAsync("日本語");
    await Pages.SettingsPage.ClickUpdateSettingsButtonAsync();

    // Assert — success message should be in Japanese
    var japaneseSuccess = Page.GetByText("設定が正常に更新されました");
    await Expect(japaneseSuccess).ToBeVisibleAsync();

    // Sidebar nav should be translated to Japanese
    var japaneseNavSettings = Page.GetByRole(AriaRole.Link, new() { Name = "設定" });
    await Expect(japaneseNavSettings).ToBeVisibleAsync();
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act — logout via sidebar link (available on any page)
    await Pages.HomePage.LogoutAsync();

    // Assert
    await Expect(Pages.HomePage.SignInLink).ToBeVisibleAsync();
  }
}
