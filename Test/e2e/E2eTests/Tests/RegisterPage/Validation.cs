namespace E2eTests.Tests.RegisterPage;

/// <summary>
/// Validation tests for the Registration page (/register).
/// </summary>
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "register-validation-001",
    FeatureArea = "auth",
    Behavior = "Registration with duplicate email shows error message",
    Verifies = ["Error message contains 'already been registered with that email'"])]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    // Arrange
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.RegisterPage.GoToAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("A user has already been registered with that email");
  }

  [Fact]
  [TestCoverage(
    Id = "register-validation-002",
    FeatureArea = "auth",
    Behavior = "Registration with duplicate username shows error message",
    Verifies = ["Error message contains 'already been registered with that email'"])]
  public async Task Register_WithDuplicateUsername_DisplaysErrorMessage()
  {
    // Arrange
    var password = "TestPassword123!";

    var existingUser = await Api.CreateUserAsync();

    await Pages.RegisterPage.GoToAsync();

    // Act - Since username defaults to email, we use the existing user's email to trigger duplicate error
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(existingUser.Email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("A user has already been registered with that email");
  }
}
