namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Happy path tests for the Settings page (/settings).
/// </summary>
[Collection("E2E Tests")]
public class SettingsPageHappyPathTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanEditProfile()
  {
    // Register a user
    await RegisterUserAsync();

    // Navigate to settings to edit profile
    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();
    await settingsPage.WaitForPageToLoadAsync();

    // Update bio
    var bioText = "This is my updated bio for E2E test";
    await settingsPage.UpdateAndSaveBioAsync(bioText);

    // Go back to profile to verify bio was updated
    var profilePage = GetProfilePage();
    await profilePage.GoToAsync(TestUsername);
    await profilePage.WaitForProfileToLoadAsync(TestUsername);

    // Verify bio is displayed
    await profilePage.VerifyBioVisibleAsync(bioText);
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    // Register and login a user
    await RegisterUserAsync();

    // Navigate to settings and logout
    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();
    await settingsPage.LogoutAsync();

    // Verify user is logged out
    await settingsPage.VerifyLoggedOutAsync();
  }
}
