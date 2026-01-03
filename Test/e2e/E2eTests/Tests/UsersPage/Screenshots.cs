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
    var user2 = await Api.InviteUserAsync(user1.Token);
    var user3 = await Api.InviteUserAsync(user1.Token);

    // Act - Log in and navigate to users page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);
    await Pages.UsersPage.GoToAsync();

    // Wait for users to be visible
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Assert all table column headers are visible
    await Expect(Page.GetByText("Username", new() { Exact = true })).ToBeVisibleAsync();
    await Expect(Page.GetByText("Email", new() { Exact = true })).ToBeVisibleAsync();

    // Assert user1's data is visible in the table
    await Expect(Pages.UsersPage.GetUserRowByUsername(user1.Email)).ToBeVisibleAsync();

    // Take screenshot of the full page
    var screenshotPath = await TakeScreenshotAsync();

    // Assert that screenshot width does not exceed viewport width
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);

    // Assert we're on the users page
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/users");
  }
}
