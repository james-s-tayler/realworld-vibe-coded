namespace E2eTests.Tests.HomePage;

/// <summary>
/// Happy path tests for the Home page (/) including feeds and pagination.
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  private const int TotalArticles = 50;

  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task CreatedArticle_AppearsInGlobalFeed()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert - INTENTIONALLY BROKEN to test CI sticky comments
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title + " - WRONG");

    await Pages.HomePage.ClickArticleAsync(article.Title);
    await Pages.ArticlePage.VerifyArticleTitleAsync(article.Title);
  }

  [Fact]
  public async Task GlobalFeed_IsSelectedByDefaultForUnauthenticatedUser()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { "default-test" });

    // Act
    await Pages.HomePage.GoToAsync();

    // Assert
    await Pages.HomePage.VerifyGlobalFeedIsSelectedAsync();
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange - create user and articles via API
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, TotalArticles);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleCountAsync(20);

    // Act
    await Pages.HomePage.VerifyPaginationVisibleAsync();
    await Pages.HomePage.ClickNextPageAsync();
    await Pages.HomePage.ClickNextPageAsync();
    await Pages.HomePage.ClickPreviousPageAsync();
    await Pages.HomePage.VerifyArticleCountAsync(20);
    await Pages.HomePage.ClickPreviousPageAsync();

    // Assert
    await Pages.HomePage.VerifyArticleCountAsync(20);
    await Pages.HomePage.VerifyBackwardButtonDisabledAsync();
  }

  [Fact]
  public async Task YourFeed_ShowsArticlesFromFollowedUsers()
  {
    // Arrange - create two users, article, and follow relationship via API
    var user1 = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.CreateUserAsync();
    await Api.FollowUserAsync(user2.Token, user1.Username);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickYourFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  public async Task UserCanFilterArticlesByTag()
  {
    // Arrange - create user and article with tag via API
    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { testTag });

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickSidebarTagAsync(testTag);

    // Assert
    await Pages.HomePage.VerifyTagFilterTabVisibleAsync(testTag);
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    // Arrange - create user and few articles via API
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, 5);

    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticlesLoadedAsync();
    await Pages.HomePage.VerifyPaginationVisibleAsync();
  }

  [Fact]
  public async Task YourFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user1.Token, TotalArticles);

    var user2 = await Api.CreateUserAsync();
    await Api.FollowUserAsync(user2.Token, user1.Username);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    // Act
    await Pages.HomePage.ClickYourFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticleCountAsync(20);
    await Pages.HomePage.VerifyPaginationVisibleAsync();
    await Pages.HomePage.ClickNextPageAsync();
    await Pages.HomePage.ClickNextPageAsync();
    await Pages.HomePage.VerifyArticleCountAsync(10);
    await Pages.HomePage.ClickPreviousPageAsync();
    await Pages.HomePage.VerifyArticleCountAsync(20);
  }
}
