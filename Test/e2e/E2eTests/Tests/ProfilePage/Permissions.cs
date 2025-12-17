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
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticleFromHomePage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user.Token);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickFavoriteButtonOnPreviewAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user.Token);

    await Pages.ProfilePage.GoToAsync(user.Email);
    await Pages.ProfilePage.WaitForProfileToLoadAsync(user.Email);

    // Act
    await Pages.ProfilePage.ClickFollowButtonWithoutWaitAsync(user.Email);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
