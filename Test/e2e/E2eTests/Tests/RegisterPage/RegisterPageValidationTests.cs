namespace E2eTests.Tests.RegisterPage;

/// <summary>
/// Validation tests for the Registration page (/register).
/// </summary>
[Collection("E2E Tests")]
public class RegisterPageValidationTests : ConduitPageTest
{
  [Fact]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Register the first user
    var timestamp = DateTime.UtcNow.Ticks;
    var email = $"duplicate{timestamp}@test.com";
    var username1 = $"user1_{timestamp}";
    var username2 = $"user2_{timestamp}";
    var password = "TestPassword123!";

    // Register first user
    await RegisterUserAsync(username1, email, password);

    // Sign out
    await SignOutAsync();

    // Try to register a second user with the same email
    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickSignUpAsync();

    var registerPage = GetRegisterPage();
    await registerPage.RegisterAndExpectErrorAsync(username2, email, password);

    // Verify the error contains the validation message about email already existing
    await registerPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
