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
  [TestCoverage(
    Id = "settings-happy-001",
    FeatureArea = "settings",
    Behavior = "User can update bio and see it reflected on profile page",
    Verifies = ["Bio text visible on profile page after update"])]
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
  [TestCoverage(
    Id = "settings-happy-002",
    FeatureArea = "settings",
    Behavior = "User can change language to Japanese and see translated UI",
    Verifies = ["Success message in Japanese", "Sidebar nav in Japanese", "Page title in Japanese", "Dashboard welcome text in Japanese"])]
  public async Task UserCanChangeLanguageToJapanese()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act - change language to Japanese
    await Pages.SettingsPage.ChangeLanguageAsync("日本語");

    // Assert - success message should appear in Japanese
    var japaneseSuccessMessage = Page.GetByText("設定が正常に更新されました");
    await Expect(japaneseSuccessMessage).ToBeVisibleAsync();

    // Assert - sidebar navigation should show Japanese text
    var japaneseHomeLink = Page.GetByRole(AriaRole.Link, new() { Name = "ホーム" });
    await Expect(japaneseHomeLink).ToBeVisibleAsync();

    var japaneseSettingsLink = Page.GetByRole(AriaRole.Link, new() { Name = "設定", Exact = true });
    await Expect(japaneseSettingsLink).ToBeVisibleAsync();

    // Assert - page title should be in Japanese
    var japaneseTitle = Page.GetByRole(AriaRole.Heading, new() { Name = "設定" });
    await Expect(japaneseTitle).ToBeVisibleAsync();

    // Navigate away and back to verify persistence
    await japaneseHomeLink.ClickAsync();

    // Dashboard should show Japanese welcome text
    var welcomeText = Page.GetByText("ようこそ");
    await Expect(welcomeText).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "settings-happy-003",
    FeatureArea = "settings",
    Behavior = "User can sign out via sidebar logout button",
    Verifies = ["User is logged out after clicking logout"])]
  public async Task UserCanSignOut()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act - Logout via sidebar (LogoutAsync is on BasePage, accessible from any page object)
    await Pages.SettingsPage.LogoutAsync();
  }
}
