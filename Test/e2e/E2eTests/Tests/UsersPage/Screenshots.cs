namespace E2eTests.Tests.UsersPage;

/// <summary>
/// Screenshot tests for the Users page (/users).
/// </summary>
public class Screenshots : AppPageTest
{
  public Screenshots(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UsersPageWithMultipleUsers()
  {
    // Arrange - create one user and invite two others to same tenant
    var user1 = await Api.CreateUserWithMaxLengthsAsync();
    await Api.InviteUserAsync(user1.Token);
    await Api.InviteUserAsync(user1.Token);

    // Act - Log in and navigate to users page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);
    await Pages.UsersPage.GoToAsync();

    // Wait for users to be visible
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Assert all table column headers are visible
    await Expect(Page.Locator("thead").GetByText("Username", new() { Exact = true })).ToBeVisibleAsync();
    await Expect(Page.Locator("thead").GetByText("Email", new() { Exact = true })).ToBeVisibleAsync();

    // Assert user1's data is visible in the table
    await Expect(Pages.UsersPage.GetUserRowByUsername(user1.Email)).ToBeVisibleAsync();

    // Take screenshot of the full page
    var screenshotPath = await TakeScreenshotAsync();

    // Assert that screenshot width does not exceed viewport width
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);

    // Assert we're on the users page
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/users");
  }

  [Fact]
  public async Task UsersPageWithPagination()
  {
    // Arrange - create admin and invite a user
    var admin = await Api.CreateUserAsync();
    await Api.InviteUserAsync(admin.Token);

    // Act
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.Pagination).ToBeVisibleAsync();

    // Take screenshot
    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  public async Task UsersPageWithDeactivatedUser()
  {
    // Arrange - create admin, invite user, then deactivate them
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);
    var invitedUserId = await Api.GetUserIdByEmailAsync(admin.Token, invited.Email);
    await Api.DeactivateUserAsync(admin.Token, invitedUserId);

    // Act
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.GetUserStatusTag(invited.Email)).ToContainTextAsync("Deactivated");

    // Take screenshot
    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }

  [Fact]
  public async Task EditRolesModalOpen()
  {
    // Arrange
    var admin = await Api.CreateUserAsync();
    var invited = await Api.InviteUserAsync(admin.Token);

    // Act
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(admin.Email, admin.Password);
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Pages.UsersPage.OpenEditRolesModalAsync(invited.Email);

    // Take screenshot with modal open
    var screenshotPath = await TakeScreenshotAsync();
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);
  }
}
