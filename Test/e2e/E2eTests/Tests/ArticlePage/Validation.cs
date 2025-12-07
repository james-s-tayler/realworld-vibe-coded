namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Validation tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task AddComment_WithEmptyBody_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act
    await Pages.ArticlePage.AddCommentAndExpectErrorAsync(string.Empty);

    // Assert
    await Pages.ArticlePage.VerifyCommentErrorContainsTextAsync("can't be blank");
  }
}
