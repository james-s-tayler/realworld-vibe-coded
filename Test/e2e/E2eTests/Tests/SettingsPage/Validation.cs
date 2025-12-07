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
  public async Task UpdateSettings_WithUsernameTooShort_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UsernameInput.ClearAsync();
    await Pages.SettingsPage.UsernameInput.FillAsync("a"); // Only 1 character, minimum is 2
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync();

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("must be at least 2 characters");
  }

  [Fact]
  public async Task UpdateSettings_WithUsernameTooLong_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.UsernameInput.ClearAsync();
    await Pages.SettingsPage.UsernameInput.FillAsync(new string('a', 101)); // 101 characters, maximum is 100
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync();

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("cannot exceed 100 characters");
  }

  [Fact]
  public async Task UpdateSettings_WithInvalidEmail_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.EmailInput.ClearAsync();
    await Pages.SettingsPage.EmailInput.FillAsync("invalidemail"); // Missing @ and domain
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync();

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("is invalid");
  }

  [Fact]
  public async Task UpdateSettings_WithPasswordTooShort_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.PasswordInput.FillAsync("12345"); // Only 5 characters, minimum is 6
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync();

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("must be at least 6 characters");
  }

  [Fact]
  public async Task UpdateSettings_WithBioTooLong_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.SettingsPage.GoToAsync();

    // Act
    await Pages.SettingsPage.BioInput.FillAsync(new string('a', 1001)); // 1001 characters, maximum is 1000
    await Pages.SettingsPage.UpdateSettingsAndExpectErrorAsync();

    // Assert
    await Pages.SettingsPage.VerifyErrorContainsTextAsync("cannot exceed 1000 characters");
  }
}
