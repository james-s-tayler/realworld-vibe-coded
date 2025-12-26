namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Permission tests for the Profile page (/profile/:username).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenAccessingHomePage()
  {
    // Act - try to access home page without authentication
    await Pages.HomePage.GoToAsync();

    // Assert - should redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenAccessingProfilePage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    // Act - try to access profile page without authentication
    await Pages.ProfilePage.GoToAsync(user.Email);

    // Assert - should redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
