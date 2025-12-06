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
    // Arrange - create user via API with a specific email
    var timestamp = DateTime.UtcNow.Ticks;
    var username2 = $"user2_{timestamp}";
    var password = "TestPassword123!";

    // Create first user with an email
    var existingUser = await Api.CreateUserAsync();

    // Now use that same email for the second registration
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act - try to register with the same email
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username2, existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
