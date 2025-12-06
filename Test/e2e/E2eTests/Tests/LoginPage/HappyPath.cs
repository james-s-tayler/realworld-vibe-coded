namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Happy path tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    // Arrange
    await RegisterUserAsync();

    await SignOutAsync();

    var loginPage = GetLoginPage();
    await loginPage.GoToAsync();

    // Act
    var homePage = await loginPage.LoginAsync(TestEmail, TestPassword);

    // Assert
    await Expect(homePage.GetUserProfileLink(TestUsername)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
