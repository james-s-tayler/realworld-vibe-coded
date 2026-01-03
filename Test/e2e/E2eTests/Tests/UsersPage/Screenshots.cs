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
    // Arrange - create multiple users
    var user1 = await Api.CreateUserWithMaxLengthsAsync();
    var user2 = await Api.CreateUserAsync();
    var user3 = await Api.CreateUserAsync();

    // Act - Log in and navigate to users page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user1.Email, user1.Password);
    await Pages.UsersPage.GoToAsync();

    // Wait for users to be visible
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();

    // Take screenshot of the full page
    var screenshotPath = await TakeScreenshotAsync();

    // Assert that screenshot width does not exceed viewport width
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);

    // Assert we're on the users page
    await Expect(Page).ToHaveURLAsync(BaseUrl + "/users");
  }
}
