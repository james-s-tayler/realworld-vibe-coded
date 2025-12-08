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
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act
    await Pages.ArticlePage.ClickFavoriteButtonWithoutWaitAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act
    await Pages.ArticlePage.ClickFollowButtonWithoutWaitAsync(user.Username);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
