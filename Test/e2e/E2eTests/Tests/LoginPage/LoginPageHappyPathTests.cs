namespace E2eTests.Tests.LoginPage;

/// <summary>
/// Happy path tests for the Login page (/login).
/// </summary>
[Collection("E2E Tests")]
public class LoginPageHappyPathTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign In Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, register a user
      await RegisterUserAsync();

      // Sign out
      await SignOutAsync();

      // Now sign in with the credentials using page model
      var loginPage = GetLoginPage();
      await loginPage.GoToAsync();
      var homePage = await loginPage.LoginAsync(TestEmail, TestPassword);

      // Verify user is logged in
      Assert.True(await homePage.IsUserLoggedInAsync(TestUsername), "User should be logged in after sign in");
    }
    finally
    {
      await SaveTrace("sign_in_test");
    }
  }
}
