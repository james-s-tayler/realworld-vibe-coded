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
  [TestCoverage(
    Id = "users-happy-001",
    FeatureArea = "users",
    Behavior = "Admin can navigate to Users page from sidebar menu",
    Verifies = ["Users heading visible", "Invite User button visible"])]
  public async Task NavigateToUsersPageFromMenu()
  {
    // Arrange - create and log in a user
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act - Click the Users link from the sidebar navigation
    await Pages.UsersPage.ClickUsersAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.InviteUserButton).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "users-happy-002",
    FeatureArea = "users",
    Behavior = "Admin can invite a user who can then log in",
    Verifies = ["Invited user appears in table", "Invited user can log in and reach home page"])]
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

    // Logout via sidebar
    await Pages.UsersPage.LogoutAsync();

    // Login as the invited user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(invitedEmail, invitedPassword);

    // Assert - Verify we're logged in successfully
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/");
  }

  [Fact]
  [TestCoverage(
    Id = "users-happy-003",
    FeatureArea = "users",
    Behavior = "Clicking a user's profile link navigates to their profile page",
    Verifies = ["URL changes to /profile/{username}"])]
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
  [TestCoverage(
    Id = "users-happy-004",
    FeatureArea = "users",
    Behavior = "Deactivated user shows 'Deactivated' status in the users table",
    Verifies = ["Status tag contains 'Deactivated'"])]
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
  [TestCoverage(
    Id = "users-happy-005",
    FeatureArea = "users",
    Behavior = "Admin can deactivate and reactivate a user via the UI",
    Verifies = ["Status shows 'Deactivated' after deactivation", "Status shows 'Active' after reactivation"])]
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
  [TestCoverage(
    Id = "users-happy-006",
    FeatureArea = "users",
    Behavior = "Deactivated user cannot log in and sees lockout error",
    Verifies = ["Error message contains 'Account is locked out'"])]
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
  [TestCoverage(
    Id = "users-happy-007",
    FeatureArea = "users",
    Behavior = "Admin can add ADMIN role to a user via edit roles modal",
    Verifies = ["User row shows 'ADMIN' in roles column"])]
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
  [TestCoverage(
    Id = "users-happy-008",
    FeatureArea = "users",
    Behavior = "Admin can remove ADMIN role from a user via edit roles modal",
    Verifies = ["User row no longer shows 'ADMIN'", "User row still shows 'USER'"])]
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
  [TestCoverage(
    Id = "users-happy-009",
    FeatureArea = "users",
    Behavior = "OWNER role checkbox is read-only in edit roles modal",
    Verifies = ["OWNER checkbox is disabled"])]
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
  [TestCoverage(
    Id = "users-happy-010",
    FeatureArea = "users",
    Behavior = "Pagination displays correct total user count",
    Verifies = ["Pagination is visible", "Pagination contains correct count"])]
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
