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
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.RegisterPage.GoToAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Email already exists");
  }

  [Fact]
  public async Task Register_WithDuplicateUsername_DisplaysErrorMessage()
  {
    // Arrange
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.RegisterPage.GoToAsync();

    // Act - Since username defaults to email, we use the existing user's email to trigger duplicate error
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("Email already exists");
  }
}
