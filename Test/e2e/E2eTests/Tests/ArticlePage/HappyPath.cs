namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Happy path tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanDeleteOwnArticle()
  {
    // Arrange - create user and article via API
    var username = GenerateUniqueUsername("articleuser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act
    await Pages.ArticlePage.DeleteArticleAsync();

    // ToDo: this should actually try to access the article page via its slug and assert a not found error message appears
    // Assert
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleNotVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    // Arrange - create two users and one article via API
    var user1Username = GenerateUniqueUsername("articleuser1");
    var user1Email = GenerateUniqueEmail(user1Username);
    var user1Password = "TestPassword123!";
    var (user1Token, _) = await Api.CreateUserAsync(user1Username, user1Email, user1Password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      user1Token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var user2Username = GenerateUniqueUsername("articleuser2");
    var user2Email = GenerateUniqueEmail(user2Username);
    var user2Password = "TestPassword123!";
    var (user2Token, _) = await Api.CreateUserAsync(user2Username, user2Email, user2Password);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    await Pages.ArticlePage.ClickFavoriteButtonAsync();
    await Pages.ArticlePage.ClickUnfavoriteButtonAsync();
  }

  [Fact]
  public async Task UserCanAddCommentToArticle()
  {
    // Arrange - create user and article via API
    var username = GenerateUniqueUsername("articleuser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    var commentText = "This is a test comment from E2E tests!";
    await Pages.ArticlePage.AddCommentAsync(commentText);
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    // Arrange - create user, article, and comment via API
    var username = GenerateUniqueUsername("articleuser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var commentText = "This comment will be deleted!";
    var commentId = await Api.CreateCommentAsync(token, articleSlug, commentText);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    await Pages.ArticlePage.DeleteCommentAsync(commentText);
  }
}
