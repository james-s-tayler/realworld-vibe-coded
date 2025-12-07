namespace E2eTests.Tests.SettingsPage;

/// <summary>
/// Validation tests for the Settings page (/settings).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UpdateSettings_WithDuplicateUsername_DisplaysErrorMessage()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync();
    var user2 = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync(username: user2.Username);

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("Username already exists");
  }

  [Fact]
  public async Task UpdateSettings_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync();
    var user2 = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync(email: user2.Email);

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
