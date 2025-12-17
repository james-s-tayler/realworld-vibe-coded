namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Happy path tests for the Settings page (/settings).
/// </summary>
[Collection("E2E Tests")]
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
  public async Task UserCanSignOut()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.LogoutAsync();

    // Assert
    await Pages.SettingsPage.VerifyLoggedOutAsync();
  }
}
