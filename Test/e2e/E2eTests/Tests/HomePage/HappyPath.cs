namespace E2eTests.Tests.HomePage;

/// <summary>
/// Happy path tests for the Home page (/) including feeds and pagination.
/// </summary>
public class HappyPath : AppPageTest
{
  private const int TotalArticles = 50;

  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-001",
    FeatureArea = "feed",
    Behavior = "A newly created article appears in the global feed and can be clicked to view",
    Verifies = ["article title is visible in global feed", "clicking article navigates to article page with correct title"])]
  public async Task CreatedArticle_AppearsInGlobalFeed()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    // Log in to access the home page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title);

    await Pages.HomePage.ClickArticleAsync(article.Title);
    await Pages.ArticlePage.VerifyArticleTitleAsync(article.Title);
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-002",
    FeatureArea = "feed",
    Behavior = "Your Feed tab is selected by default for authenticated users on the home page",
    Verifies = ["Your Feed tab is the active/selected tab"])]
  public async Task YourFeed_IsSelectedByDefaultForAuthenticatedUser()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { "default-test" });

    // Act - Login as the user
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();

    // Assert - Your Feed should be selected by default for authenticated users
    await Pages.HomePage.VerifyYourFeedIsSelectedAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-003",
    FeatureArea = "feed",
    Behavior = "Global feed pagination displays correctly and navigates between pages",
    Verifies = ["initial page shows 20 articles", "pagination controls are visible", "navigating forward and back returns to first page with 20 articles", "backward button is disabled on first page"])]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange - create user and articles via API
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, TotalArticles);

    // Log in to access the home page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
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
  [TestCoverage(
    Id = "home-happy-004",
    FeatureArea = "feed",
    Behavior = "Your Feed shows articles from followed users within the same tenant",
    Verifies = ["followed user's article is visible in Your Feed"])]
  public async Task YourFeed_ShowsArticlesFromFollowedUsers()
  {
    // Arrange - create two users IN THE SAME TENANT, article, and follow relationship via API
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    var article = await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant
    await Api.FollowUserAsync(user2.Token, user1.Email);

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
  [TestCoverage(
    Id = "home-happy-005",
    FeatureArea = "feed",
    Behavior = "Article tags appear in the article preview card on the global feed",
    Verifies = ["both tags are visible in the article preview"])]
  public async Task Tags_AppearInArticlePreview_OnGlobalFeed()
  {
    // Arrange
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { tag1, tag2 });

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticlePreviewTagsAsync(article.Title, tag1, tag2);
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-006",
    FeatureArea = "feed",
    Behavior = "Article tags appear in the article preview card on Your Feed",
    Verifies = ["both tags are visible in the article preview on Your Feed"])]
  public async Task Tags_AppearInArticlePreview_OnYourFeed()
  {
    // Arrange
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var user1 = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user1.Token, new[] { tag1, tag2 });

    var user2 = await Api.InviteUserAsync(user1.Token);
    await Api.FollowUserAsync(user2.Token, user1.Email);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickYourFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticlePreviewTagsAsync(article.Title, tag1, tag2);
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-007",
    FeatureArea = "feed",
    Behavior = "Tags from created articles appear in the sidebar popular tags section",
    Verifies = ["tag is visible in the sidebar"])]
  public async Task CreatedArticleTags_AppearInSidebar_PopularTags()
  {
    // Arrange
    var tag = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var user = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user.Token, new[] { tag });

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();

    // Assert
    await Expect(Pages.HomePage.GetSidebarTag(tag)).ToBeVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-008",
    FeatureArea = "feed",
    Behavior = "Clicking a sidebar tag filters the feed to show only articles with that tag",
    Verifies = ["tag filter tab is visible", "article with the tag is visible in filtered results"])]
  public async Task UserCanFilterArticlesByTag()
  {
    // Arrange - create user and article with tag via API
    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { testTag });

    // Log in to access the home page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickSidebarTagAsync(testTag);

    // Assert
    await Pages.HomePage.VerifyTagFilterTabVisibleAsync(testTag);
    await Pages.HomePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-009",
    FeatureArea = "feed",
    Behavior = "Global feed shows pagination even with fewer articles than one full page",
    Verifies = ["articles are loaded", "pagination controls are visible"])]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    // Arrange - create user and few articles via API
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, 5);

    // Log in to access the home page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.GoToAsync();

    // Act
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert
    await Pages.HomePage.VerifyArticlesLoadedAsync();
    await Pages.HomePage.VerifyPaginationVisibleAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "home-happy-010",
    FeatureArea = "feed",
    Behavior = "Your Feed pagination displays correctly and navigates between pages",
    Verifies = ["first page shows 20 articles", "pagination controls are visible", "last page shows remaining 10 articles", "navigating back shows 20 articles again"])]
  public async Task YourFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    await Api.CreateArticlesAsync(user1.Token, TotalArticles);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant
    await Api.FollowUserAsync(user2.Token, user1.Email);

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
