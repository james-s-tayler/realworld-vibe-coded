namespace E2eTests.Tests;

/// <summary>
/// Tests for the Registration page (/register).
/// </summary>
[Collection("E2E Tests")]
public class RegisterPageTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanSignUp_AndNavigateToProfile()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign Up Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Navigate to home page
      var homePage = GetHomePage();
      await homePage.GoToAsync();

      // Click on Sign up link
      await homePage.ClickSignUpAsync();

      // Fill in registration form using page model
      var registerPage = GetRegisterPage();
      var resultHomePage = await registerPage.RegisterAsync(TestUsername, TestEmail, TestPassword);

      // Verify user is logged in
      Assert.True(await resultHomePage.IsUserLoggedInAsync(TestUsername), "User link should be visible in header after sign up");

      // Click on user profile link
      await resultHomePage.ClickUserProfileAsync(TestUsername);

      // Verify profile page elements using profile page model
      var profilePage = GetProfilePage();
      await profilePage.VerifyProfileHeadingAsync(TestUsername);
    }
    finally
    {
      await SaveTrace("user_signup_test");
    }
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Duplicate Email Registration Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register the first user
      var timestamp = DateTime.UtcNow.Ticks;
      var email = $"duplicate{timestamp}@test.com";
      var username1 = $"user1_{timestamp}";
      var username2 = $"user2_{timestamp}";
      var password = "TestPassword123!";

      // Register first user
      await RegisterUserAsync(username1, email, password);

      // Sign out
      await SignOutAsync();

      // Try to register a second user with the same email
      var homePage = GetHomePage();
      await homePage.GoToAsync();
      await homePage.ClickSignUpAsync();

      var registerPage = GetRegisterPage();
      await registerPage.RegisterAndExpectErrorAsync(username2, email, password);

      // Verify the error contains the validation message about email already existing
      await registerPage.VerifyErrorContainsTextAsync("Email already exists");
    }
    finally
    {
      await SaveTrace("duplicate_email_registration_test");
    }
  }
}
