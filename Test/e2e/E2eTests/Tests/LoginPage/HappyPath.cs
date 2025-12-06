namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Happy path tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    // Arrange - create user via API
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Assert
    await Expect(Pages.HomePage.GetUserProfileLink(user.Username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
