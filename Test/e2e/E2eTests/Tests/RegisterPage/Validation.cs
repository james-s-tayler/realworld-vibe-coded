namespace E2eTests.Tests.RegisterPage;

/// <summary>
/// Validation tests for the Registration page (/register).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.Ticks;
    var username2 = $"user2_{timestamp}";
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username2, existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Email already exists");
  }

  [Fact]
  public async Task Register_WithDuplicateUsername_DisplaysErrorMessage()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.Ticks;
    var email2 = $"user2_{timestamp}@test.com";
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(existingUser.Username, email2, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Username already exists");
  }
}
