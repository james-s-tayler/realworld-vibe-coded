namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Happy path tests for the Settings page (/settings).
/// </summary>
[Collection("E2E Tests")]
public class SettingsPageHappyPathTests : AppPageTest
{
  [Fact]
  public async Task UserCanEditProfile()
  {
    // Arrange
    await RegisterUserAsync();

    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();

    var bioText = "This is my updated bio for E2E test";

    // Act
    await settingsPage.UpdateAndSaveBioAsync(bioText);

    // Assert
    var profilePage = GetProfilePage();
    await profilePage.GoToAsync(TestUsername);
    await profilePage.WaitForProfileToLoadAsync(TestUsername);

    await profilePage.VerifyBioVisibleAsync(bioText);
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    // Arrange
    await RegisterUserAsync();

    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();

    // Act
    await settingsPage.LogoutAsync();

    // Assert
    await settingsPage.VerifyLoggedOutAsync();
  }
}
