
namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Happy path tests for the Settings page (/settings).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  [Fact]
  public async Task UserCanEditProfile()
  {
    // Arrange
    await RegisterUserAsync();

    await Pages.SettingsPage.GoToAsync();

    var bioText = "This is my updated bio for E2E test";

    // Act
    await Pages.SettingsPage.UpdateAndSaveBioAsync(bioText);

    // Assert
    await Pages.ProfilePage.GoToAsync(TestUsername);
    await Pages.ProfilePage.WaitForProfileToLoadAsync(TestUsername);

    await Pages.ProfilePage.VerifyBioVisibleAsync(bioText);
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    // Arrange
    await RegisterUserAsync();

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.LogoutAsync();

    // Assert
    await Pages.SettingsPage.VerifyLoggedOutAsync();
  }
}
