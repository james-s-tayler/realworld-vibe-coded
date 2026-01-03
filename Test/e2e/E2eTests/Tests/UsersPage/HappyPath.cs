namespace E2eTests.Tests.UsersPage;

/// <summary>
/// Happy path tests for the Users page (/users).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task NavigateToUsersPageFromMenu()
  {
    // Arrange - create and log in a user
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act - Click the Users link from the navigation menu
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickUsersAsync();

    // Assert - Verify we're on the users page
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/users");
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.InviteUserButton).ToBeVisibleAsync();
  }

  [Fact]
  public async Task InviteUserThenLogoutAndLoginAsInvitedUser()
  {
    // Arrange - create and log in an admin user
    var adminUser = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(adminUser.Email, adminUser.Password);

    // Navigate to users page
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Invite a new user
    var invitedEmail = $"invited-{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var invitedPassword = "password123";
    await Pages.UsersPage.InviteUserAsync(invitedEmail, invitedPassword);

    // Wait for the user to appear in the table
    await Pages.UsersPage.VerifyUserVisibleAsync(invitedEmail);

    // Logout
    await Pages.SettingsPage.GoToAsync();
    await Pages.SettingsPage.LogoutAsync();
    await Expect(Pages.SettingsPage.SignInLink).ToBeVisibleAsync();

    // Login as the invited user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(invitedEmail, invitedPassword);

    // Assert - Verify we're logged in successfully
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/");
    await Expect(Pages.HomePage.GlobalFeedTab).ToBeVisibleAsync();
  }

  [Fact]
  public async Task ClickUserProfileLinkNavigatesToProfile()
  {
    // Arrange - create one user and invite another to same tenant
    var user1 = await Api.CreateUserAsync();
    var user2 = await Api.InviteUserAsync(user1.Token);

    // Update user2 with a specific username so we can find them easily
    var updatedUser2 = await Api.UpdateUserAsync(user2.Token, username: $"testuser-{Guid.NewGuid().ToString("N")[..8]}");

    // Log in as user1
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    // Navigate to users page
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Wait for user2 to appear in the table (by username)
    await Expect(Pages.UsersPage.GetUserRowByUsername(updatedUser2.Username)).ToBeVisibleAsync();

    // Act - Click on user2's profile link (link text is the username)
    await Pages.UsersPage.GetUserProfileLink(updatedUser2.Username).ClickAsync();

    // Assert - Verify we're on user2's profile page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/profile/{updatedUser2.Username}");
  }
}
