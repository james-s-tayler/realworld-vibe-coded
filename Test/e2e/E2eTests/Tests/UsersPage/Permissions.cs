using Microsoft.Playwright;

namespace E2eTests.Tests.UsersPage;

/// <summary>
/// Permission tests for the Users page (/users).
/// Tests role-based access control for ADMIN features.
/// </summary>
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
    await Expect(Pages.HomePage.GlobalFeedTab).ToBeVisibleAsync();
  }
}
