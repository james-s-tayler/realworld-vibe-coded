using Microsoft.Playwright;

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

    // Act - Click the Users link from the navigation menu on the dashboard
    await Page.GotoAsync(BaseUrl);
    await Page.GetByRole(AriaRole.Link, new() { Name = "Users", Exact = true }).ClickAsync();

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
  }

  [Fact]
  public async Task ClickUserProfileLinkNavigatesToProfile()
  {
    // Arrange - create one user and invite another to same tenant
    var user1 = await Api.CreateUserAsync();
    var user2Email = $"testuser-{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var user2 = await Api.InviteUserAsync(user1.Token, user2Email);

    // Log in as user1
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);

    // Navigate to users page
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Wait for user2 to appear in the table (by username which is the email)
    await Expect(Pages.UsersPage.GetUserRowByUsername(user2.Email)).ToBeVisibleAsync();

    // Act - Click on user2's profile link (link text is the username/email)
    await Pages.UsersPage.GetUserProfileLink(user2.Email).ClickAsync();

    // Assert - Verify we're on user2's profile page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/profile/{user2.Email}");
  }

  [Fact]
  public async Task DeactivateUserThenVerifyStatusColumn()
  {
    // Arrange - create admin and invite a user
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);
    var invitedUserId = await Api.GetUserIdByEmailAsync(admin.Token, invited.Email);

    // Deactivate the user via API
    await Api.DeactivateUserAsync(admin.Token, invitedUserId);

    // Act - Log in as admin and navigate to users page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Assert - Verify deactivated user shows "Deactivated" status
    await Expect(Pages.UsersPage.GetUserStatusTag(invited.Email)).ToContainTextAsync("Deactivated");
  }

  [Fact]
  public async Task DeactivateAndReactivateUser()
  {
    // Arrange
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);
    var invitedUserId = await Api.GetUserIdByEmailAsync(admin.Token, invited.Email);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Deactivate via UI
    await Pages.UsersPage.ClickUserAction(invited.Email, "Deactivate");

    // Assert deactivated
    await Expect(Pages.UsersPage.GetUserStatusTag(invited.Email)).ToContainTextAsync("Deactivated");

    // Act - Reactivate via UI
    await Pages.UsersPage.ClickUserAction(invited.Email, "Reactivate");

    // Assert reactivated
    await Expect(Pages.UsersPage.GetUserStatusTag(invited.Email)).ToContainTextAsync("Active");
  }

  [Fact]
  public async Task DeactivatedUserCannotLogin()
  {
    // Arrange - deactivate an invited user
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);
    var invitedUserId = await Api.GetUserIdByEmailAsync(admin.Token, invited.Email);
    await Api.DeactivateUserAsync(admin.Token, invitedUserId);

    // Act - Try to log in as the deactivated user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAndExpectErrorAsync(invited.Email, invited.Password);

    // Assert - Should see lockout error
    await Pages.LoginPage.VerifyErrorContainsTextAsync("Account is locked out");
  }

  [Fact]
  public async Task EditUserRolesAddsAdminRole()
  {
    // Arrange
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open edit roles modal and add ADMIN
    await Pages.UsersPage.OpenEditRolesModalAsync(invited.Email);
    await Pages.UsersPage.GetRoleCheckbox("ADMIN").CheckAsync(new() { Force = true });
    await Pages.UsersPage.SaveEditRolesAsync();

    // Assert - Verify the roles column shows ADMIN
    await Expect(Pages.UsersPage.GetUserRowByUsername(invited.Email)).ToContainTextAsync("ADMIN");
  }

  [Fact]
  public async Task EditUserRolesRemovesAdminRole()
  {
    // Arrange - create admin, invite user, give them ADMIN role via API
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);
    var invitedUserId = await Api.GetUserIdByEmailAsync(admin.Token, invited.Email);
    await Api.UpdateUserRolesAsync(admin.Token, invitedUserId, ["ADMIN"]);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open edit roles modal and uncheck ADMIN
    await Pages.UsersPage.OpenEditRolesModalAsync(invited.Email);
    await Pages.UsersPage.GetRoleCheckbox("ADMIN").UncheckAsync(new() { Force = true });
    await Pages.UsersPage.SaveEditRolesAsync();

    // Assert - ADMIN should no longer be in the roles, USER preserved
    await Expect(Pages.UsersPage.GetUserRowByUsername(invited.Email)).Not.ToContainTextAsync("ADMIN");
    await Expect(Pages.UsersPage.GetUserRowByUsername(invited.Email)).ToContainTextAsync("USER");
  }

  [Fact]
  public async Task OwnerRoleIsReadOnlyInEditModal()
  {
    // Arrange - log in as the owner/admin
    var admin = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open edit roles modal for the owner user
    await Pages.UsersPage.OpenEditRolesModalAsync(admin.Email);

    // Assert - OWNER checkbox should be disabled
    await Expect(Pages.UsersPage.GetRoleCheckbox("OWNER")).ToBeDisabledAsync();
  }

  [Fact]
  public async Task PaginationShowsCorrectCount()
  {
    // Arrange - create admin and invite a user
    var admin = await Api.CreateUserAsync();
    await Api.InviteUserAsync(admin.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Assert - Pagination is visible and shows correct total
    await Expect(Pages.UsersPage.Pagination).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.Pagination).ToContainTextAsync("2");
  }
}
