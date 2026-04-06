using Microsoft.Playwright;

namespace E2eTests.Tests.UsersPage;

public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task AdminUserSeesUsersLinkInNavigation()
  {
    // Arrange - create and log in an ADMIN user (first user in tenant)
    var adminUser = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(adminUser.Email, adminUser.Password);

    // Act - Navigate to home page to see the navigation
    await Pages.HomePage.GoToAsync();

    // Assert - Verify Users link is visible for ADMIN
    await Expect(Pages.HomePage.UsersLink).ToBeVisibleAsync();
  }

  [Fact]
  public async Task NonAdminUserDoesNotSeeUsersLinkInNavigation()
  {
    // Arrange - create ADMIN user, then invite non-ADMIN user
    var adminUser = await Api.CreateUserAsync();
    var nonAdminEmail = $"author-{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var nonAdminUser = await Api.InviteUserAsync(adminUser.Token, nonAdminEmail);

    // Log in as non-ADMIN user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(nonAdminUser.Email, nonAdminUser.Password);

    // Act - Navigate to home page to see the navigation
    await Pages.HomePage.GoToAsync();

    // Assert - Verify Users link is NOT visible for non-ADMIN
    await Expect(Pages.HomePage.UsersLink).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task NonAdminUserRedirectedToForbiddenWhenAccessingUsersPage()
  {
    // Arrange - create ADMIN user, then invite non-ADMIN user
    var adminUser = await Api.CreateUserAsync();
    var nonAdminEmail = $"author-{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var nonAdminUser = await Api.InviteUserAsync(adminUser.Token, nonAdminEmail);

    // Log in as non-ADMIN user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(nonAdminUser.Email, nonAdminUser.Password);

    // Act - Attempt to navigate directly to /users
    await Page.GotoAsync($"{BaseUrl}/users");

    // Assert - Verify redirected to forbidden page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/forbidden");
    await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "403 - Forbidden" })).ToBeVisibleAsync();
    await Expect(Page.GetByText("You don't have permission to access this page.")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task ForbiddenPageHasLinkToHomePage()
  {
    // Arrange - create ADMIN user, then invite non-ADMIN user
    var adminUser = await Api.CreateUserAsync();
    var nonAdminEmail = $"author-{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var nonAdminUser = await Api.InviteUserAsync(adminUser.Token, nonAdminEmail);

    // Log in as non-ADMIN user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(nonAdminUser.Email, nonAdminUser.Password);

    // Navigate to forbidden page
    await Page.GotoAsync($"{BaseUrl}/users");
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/forbidden");

    // Act - Click "Go back to home" link
    await Page.GetByRole(AriaRole.Link, new() { Name = "Go back to home" }).ClickAsync();

    // Assert - Verify we're back on the home page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToBeVisibleAsync();
  }

  [Fact]
  public async Task AdminCannotDeactivateSelf()
  {
    // Arrange - log in as admin
    var admin = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open the overflow menu for self
    await Pages.UsersPage.GetUserActionsMenu(admin.Email).ClickAsync();

    // Assert - Deactivate option should not be present for self
    await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Deactivate" })).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task AdminCannotRemoveOwnAdminRole()
  {
    // Arrange - log in as admin
    var admin = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open edit roles modal for self and uncheck ADMIN, then try to save
    await Pages.UsersPage.OpenEditRolesModalAsync(admin.Email);
    await Pages.UsersPage.GetRoleCheckbox("ADMIN").UncheckAsync(new() { Force = true });
    await Pages.UsersPage.EditRolesSaveButton.ClickAsync();

    // Assert - Should show an error (Forbidden)
    await Expect(Page.GetByText("Cannot remove your own ADMIN role")).ToBeVisibleAsync();
  }

  [Fact]
  public async Task AdminCannotDeactivateSelf()
  {
    // Arrange - log in as admin
    var admin = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open the overflow menu for self
    await Pages.UsersPage.GetUserActionsMenu(admin.Email).ClickAsync();

    // Assert - Deactivate option should not be present for self
    await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Deactivate" })).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task AdminCannotRemoveOwnAdminRole()
  {
    // Arrange - log in as admin
    var admin = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Act - Open edit roles modal for self and uncheck ADMIN, then try to save
    await Pages.UsersPage.OpenEditRolesModalAsync(admin.Email);
    await Pages.UsersPage.GetRoleCheckbox("ADMIN").UncheckAsync(new() { Force = true });
    await Pages.UsersPage.EditRolesSaveButton.ClickAsync();

    // Assert - Should show an error (Forbidden)
    await Expect(Page.GetByText("Cannot remove your own ADMIN role")).ToBeVisibleAsync();
  }
}
