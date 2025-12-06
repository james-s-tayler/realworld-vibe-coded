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
    var username = GenerateUniqueUsername("homeuser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

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
    var uniqueId = GenerateUniqueUsername("deftest");
    var email = $"{uniqueId}@test.com";
    var (token, _) = await Api.CreateUserAsync(uniqueId, email, "TestPassword123!");

    var articleTitle = $"Default Tab Test Article - {uniqueId}";
    await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article for default tab selection",
      "This article verifies the global feed is selected by default",
      new[] { "default-test" });

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
    var uniqueIdPrefix = GenerateUniqueUsername("pagtest");
    var email = $"{uniqueIdPrefix}@test.com";
    var (token, _) = await Api.CreateUserAsync(uniqueIdPrefix, email, "TestPassword123!");
    await Api.CreateArticlesAsync(token, TotalArticles, uniqueIdPrefix);

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
    var user1Username = GenerateUniqueUsername("profileuser1");
    var user1Email = GenerateUniqueEmail(user1Username);
    var user1Password = "TestPassword123!";
    var (user1Token, _) = await Api.CreateUserAsync(user1Username, user1Email, user1Password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      user1Token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var user2Username = GenerateUniqueUsername("profileuser2");
    var user2Email = GenerateUniqueEmail(user2Username);
    var user2Password = "TestPassword123!";
    var (user2Token, _) = await Api.CreateUserAsync(user2Username, user2Email, user2Password);

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
    var username = GenerateUniqueUsername("profileuser1");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var articleTitle = $"Tagged Article {GenerateUniqueUsername("tag")}";
    await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article with tag",
      "This is a test article body.",
      new[] { testTag });

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
    var uniqueId = GenerateUniqueUsername("fewart");
    var email = $"{uniqueId}@test.com";
    var (token, _) = await Api.CreateUserAsync(uniqueId, email, "TestPassword123!");
    await Api.CreateArticlesAsync(token, 5, uniqueId);

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
    var user1Id = GenerateUniqueUsername("feeduser1");
    var user1Email = $"{user1Id}@test.com";
    var (user1Token, user1Username) = await Api.CreateUserAsync(user1Id, user1Email, "TestPassword123!");

    var user2Id = GenerateUniqueUsername("feeduser2");
    var user2Email = $"{user2Id}@test.com";
    var (user2Token, _) = await Api.CreateUserAsync(user2Id, user2Email, "TestPassword123!");

    await Api.CreateArticlesAsync(user1Token, TotalArticles, user1Id);

    await Api.FollowUserAsync(user2Token, user1Username);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, "TestPassword123!");

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
