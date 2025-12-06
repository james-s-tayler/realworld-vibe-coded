namespace E2eTests.Tests.RegisterPage;

/// <summary>
/// Validation tests for the Registration page (/register).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  [Fact]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.Ticks;
    var email = $"duplicate{timestamp}@test.com";
    var username1 = $"user1_{timestamp}";
    var username2 = $"user2_{timestamp}";
    var password = "TestPassword123!";

    await RegisterUserAsync(username1, email, password);

    await SignOutAsync();

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username2, email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
