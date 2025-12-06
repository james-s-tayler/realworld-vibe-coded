namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Permission tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticle()
  {
    // Arrange - create user and article via API
    var (token, username, _, _) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act
    await Pages.ArticlePage.ClickFavoriteButtonWithoutWaitAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    // Arrange - create user and article via API
    var (token, username, _, _) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act
    await Pages.ArticlePage.ClickFollowButtonWithoutWaitAsync(username);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
