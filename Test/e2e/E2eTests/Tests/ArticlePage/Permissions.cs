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
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenAccessingArticlePage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    // Act - try to access article page without authentication
    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Assert - should redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
