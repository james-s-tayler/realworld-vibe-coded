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
  public async Task Register_WithMissingUsername_DisplaysErrorMessage()
  {
    // Arrange
    var email = GenerateUniqueEmail("testuser");
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(string.Empty, email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("is required");
  }

  [Fact]
  public async Task Register_WithMissingEmail_DisplaysErrorMessage()
  {
    // Arrange
    var username = GenerateUniqueUsername("testuser");
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, string.Empty, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("is required");
  }

  [Fact]
  public async Task Register_WithMissingPassword_DisplaysErrorMessage()
  {
    // Arrange
    var username = GenerateUniqueUsername("testuser");
    var email = GenerateUniqueEmail(username);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, email, string.Empty);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("is required");
  }

  [Fact]
  public async Task Register_WithUsernameTooShort_DisplaysErrorMessage()
  {
    // Arrange
    var username = "a"; // Only 1 character, minimum is 2
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("must be at least 2 characters");
  }

  [Fact]
  public async Task Register_WithUsernameTooLong_DisplaysErrorMessage()
  {
    // Arrange
    var username = new string('a', 101); // 101 characters, maximum is 100
    var email = GenerateUniqueEmail("testuser");
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, email, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("cannot exceed 100 characters");
  }

  [Fact]
  public async Task Register_WithInvalidEmailMissingAt_DisplaysErrorMessage()
  {
    // Arrange
    var username = GenerateUniqueUsername("testuser");
    var invalidEmail = "invalidemail.com"; // Missing @
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, invalidEmail, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("is invalid");
  }

  [Fact]
  public async Task Register_WithInvalidEmailMissingDomain_DisplaysErrorMessage()
  {
    // Arrange
    var username = GenerateUniqueUsername("testuser");
    var invalidEmail = "test@"; // Missing domain
    var password = "TestPassword123!";

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, invalidEmail, password);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("is invalid");
  }

  [Fact]
  public async Task Register_WithPasswordTooShort_DisplaysErrorMessage()
  {
    // Arrange
    var username = GenerateUniqueUsername("testuser");
    var email = GenerateUniqueEmail(username);
    var shortPassword = "12345"; // Only 5 characters, minimum is 6

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();

    // Act
    await Pages.RegisterPage.RegisterAndExpectErrorAsync(username, email, shortPassword);

    // Assert
    await Pages.RegisterPage.VerifyErrorContainsTextAsync("must be at least 6 characters");
  }
}
