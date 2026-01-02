namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Screenshot tests for the Profile page (/profile/:username) with favorited articles.
/// </summary>
public class Screenshots : AppPageTest
{
  public Screenshots(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task ProfilePageWithFavoritedArticle()
  {
    // Arrange - create user with max-length fields
    var user = await Api.CreateUserWithMaxLengthsAsync();

    // Create article with max-length fields (except body = 500 chars)
    var article = await Api.CreateArticleWithMaxLengthsAsync(user.Token);

    // Add a comment with max-length body
    await Api.CreateCommentWithMaxLengthAsync(user.Token, article.Slug);

    // Favorite the article to ensure it appears in favorited tab
    await Api.FavoriteArticleAsync(user.Token, article.Slug);

    // Act - Log in to access the profile page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Navigate to profile page and view favorited articles
    await Pages.ProfilePage.GoToAsync(user.Email);
    await Pages.ProfilePage.ClickFavoritedArticlesTabAsync();

    // Wait for the favorited article to be visible
    await Expect(Pages.ProfilePage.GetArticlePreviewByTitle(article.Title)).ToBeVisibleAsync();

    // Take screenshot of the full page
    var screenshotPath = await TakeScreenshotAsync();

    // Assert that screenshot width does not exceed viewport width
    await AssertScreenshotWidthNotExceedingViewportAsync(screenshotPath);

    // Assert we're on the profile page
    await Expect(Page).ToHaveURLAsync(new Regex($"/profile/{user.Email}"));
  }
}
