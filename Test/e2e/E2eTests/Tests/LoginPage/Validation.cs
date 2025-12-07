namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Validation tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task Login_WithUnregisteredEmail_DisplaysErrorMessage()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.Ticks;
    var unregisteredEmail = $"nonexistent_{timestamp}@test.com";
    var password = "TestPassword123!";

    await Pages.LoginPage.GoToAsync();

    // Act & Assert
    await Pages.LoginPage.LoginAndExpectErrorAsync(unregisteredEmail, password);
  }

  [Fact]
  public async Task Login_WithIncorrectPassword_DisplaysErrorMessage()
  {
    // Arrange
    var existingUser = await Api.CreateUserAsync();
    var incorrectPassword = "WrongPassword123!";

    await Pages.LoginPage.GoToAsync();

    // Act & Assert
    await Pages.LoginPage.LoginAndExpectErrorAsync(existingUser.Email, incorrectPassword);
  }
}
