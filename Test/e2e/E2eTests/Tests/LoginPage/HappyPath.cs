namespace E2eTests.Tests.LoginPage;
using static E2eTests.PageModels.Pages;

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

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAsync(TestEmail, TestPassword);

    // Assert
    await Expect(Pages.HomePage.GetUserProfileLink(TestUsername)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
