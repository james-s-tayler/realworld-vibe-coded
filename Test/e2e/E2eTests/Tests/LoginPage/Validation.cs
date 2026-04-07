namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Validation tests for the Login page (/login).
/// </summary>
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "auth-validation-001",
    FeatureArea = "auth",
    Behavior = "Login with unregistered email shows error message",
    Verifies = ["Error message contains 'Invalid email or password'"])]
  public async Task Login_WithUnregisteredEmail_DisplaysErrorMessage()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.Ticks;
    var unregisteredEmail = $"nonexistent_{timestamp}@test.com";
    var password = "TestPassword123!";

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAndExpectErrorAsync(unregisteredEmail, password);

    // Assert
    await Pages.LoginPage.VerifyErrorContainsTextAsync("Invalid email or password");
  }

  [Fact]
  [TestCoverage(
    Id = "auth-validation-002",
    FeatureArea = "auth",
    Behavior = "Login with incorrect password shows error message",
    Verifies = ["Error message contains 'Invalid email or password'"])]
  public async Task Login_WithIncorrectPassword_DisplaysErrorMessage()
  {
    // Arrange
    var existingUser = await Api.CreateUserAsync();
    var incorrectPassword = "WrongPassword123!";

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAndExpectErrorAsync(existingUser.Email, incorrectPassword);

    // Assert
    await Pages.LoginPage.VerifyErrorContainsTextAsync("Invalid email or password");
  }
}
