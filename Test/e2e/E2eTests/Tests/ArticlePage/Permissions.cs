namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Permission tests for the Article page (/article/:slug).
/// </summary>
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "article-permissions-001",
    FeatureArea = "articles",
    Behavior = "Unauthenticated user is redirected to the login page when accessing an article",
    Verifies = ["URL changes to /login"])]
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
