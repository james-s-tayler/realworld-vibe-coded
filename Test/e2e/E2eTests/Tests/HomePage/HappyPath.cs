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
    var (token, username, email, password) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token);

    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticleVisibleAsync(articleTitle);

    await Pages.HomePage.ClickArticleAsync(articleTitle);
    await Pages.ArticlePage.VerifyArticleTitleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_IsSelectedByDefaultForUnauthenticatedUser()
  {
    // Arrange - create user and article via API
    var (token, _, _, _) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token, new[] { "default-test" });

    // Act
    await Pages.HomePage.GoToAsync();

    // Assert
    await Pages.HomePage.VerifyGlobalFeedIsSelectedAsync();
    await Pages.HomePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange - create user and articles via API
    var (token, _, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(token, TotalArticles);

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
    var (user1Token, user1Username, _, _) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(user1Token);

    var (user2Token, _, user2Email, user2Password) = await Api.CreateUserAsync();
    await Api.FollowUserAsync(user2Token, user1Username);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickYourFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFilterArticlesByTag()
  {
    // Arrange - create user and article with tag via API
    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var (token, _, _, _) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token, new[] { testTag });

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickSidebarTagAsync(testTag);

    // Assert
    await Pages.HomePage.VerifyTagFilterTabVisibleAsync(testTag);
    await Pages.HomePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    // Arrange - create user and few articles via API
    var (token, _, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(token, 5);

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
    // Arrange - create two users, articles, and follow relationship via API
    var (user1Token, user1Username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user1Token, TotalArticles);

    var (user2Token, _, user2Email, user2Password) = await Api.CreateUserAsync();
    await Api.FollowUserAsync(user2Token, user1Username);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

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
