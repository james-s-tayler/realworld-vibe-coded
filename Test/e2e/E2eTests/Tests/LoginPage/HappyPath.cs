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
    var (_, username, email, password) = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();

    // Act
    await Pages.LoginPage.LoginAsync(email, password);

    // Assert
    await Expect(Pages.HomePage.GetUserProfileLink(username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
