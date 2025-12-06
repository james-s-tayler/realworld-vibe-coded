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
    // Arrange - create user and article via API
    var (token, username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(token);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickFavoriteButtonOnPreviewAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
  {
    // Arrange - create user and article via API
    var (token, username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(token);

    await Pages.ProfilePage.GoToAsync(username);
    await Pages.ProfilePage.WaitForProfileToLoadAsync(username);

    // Act
    await Pages.ProfilePage.ClickFollowButtonWithoutWaitAsync(username);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }
}
