namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Permission tests for the Profile page (/profile/:username).
/// </summary>
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "profile-permissions-001",
    FeatureArea = "auth",
    Behavior = "Unauthenticated user accessing dashboard is redirected to login",
    Verifies = ["URL changes to /login"])]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenAccessingDashboard()
  {
    // Act - try to access dashboard without authentication
    await Page.GotoAsync(BaseUrl);

    // Assert - should redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  [TestCoverage(
    Id = "profile-permissions-002",
    FeatureArea = "auth",
    Behavior = "Unauthenticated user accessing profile page is redirected to login",
    Verifies = ["URL changes to /login"])]
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
