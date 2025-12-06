namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Happy path tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class LoginPageHappyPathTests : AppPageTest
{
  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    // First, register a user
    await RegisterUserAsync();

    // Sign out
    await SignOutAsync();

    // Now sign in with the credentials using page model
    var loginPage = GetLoginPage();
    await loginPage.GoToAsync();
    var homePage = await loginPage.LoginAsync(TestEmail, TestPassword);

    // Verify user is logged in by checking their profile link is visible
    await Expect(homePage.GetUserProfileLink(TestUsername)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
