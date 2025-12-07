namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Screenshot tests for the Article page (/article/:slug) with max-length content.
/// </summary>
[Collection("E2E Tests")]
public class Screenshots : AppPageTest
{
  public Screenshots(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task ArticlePageWithMaxLengthContent()
  {
    // Arrange - create user with max-length fields
    var user = await Api.CreateUserWithMaxLengthsAsync();

    // Create article with max-length fields (except body = 500 chars)
    var article = await Api.CreateArticleWithMaxLengthsAsync(user.Token);

    // Add a comment with max-length body
    await Api.CreateCommentWithMaxLengthAsync(user.Token, article.Slug);

    // Act + Assert
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Wait for the article title to be visible to ensure the page is fully loaded
    await Expect(Pages.ArticlePage.GetArticleTitle(article.Title)).ToBeVisibleAsync();

    // Take screenshot of the full page
    var screenshotPath = await TakeScreenshotAsync();

    // Assert that screenshot width does not exceed viewport width
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);

    await Expect(Page).ToHaveURLAsync(new Regex($"/article/{article.Slug}"));
  }
}
