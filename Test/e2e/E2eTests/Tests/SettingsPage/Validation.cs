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
    // Arrange - create two users in the same tenant
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync(username: user2.Email);

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("Username already exists");
  }

  [Fact]
  public async Task UpdateSettings_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Arrange - create two users in the same tenant
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync(email: user2.Email);

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
