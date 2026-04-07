namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Happy path tests for the Login page (/login).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "auth-happy-001",
    FeatureArea = "auth",
    Behavior = "User can log in with valid credentials and see authenticated UI state",
    Verifies = ["User profile link shows email after login"])]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Assert
    await Expect(Pages.HomePage.GetUserProfileLink(user.Email)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
