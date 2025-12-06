namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Happy path tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanCreateArticle_AndViewArticle()
  {
    // Arrange - create user via API
    var username = GenerateUniqueUsername("editoruser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    await Pages.HomePage.ClickNewArticleAsync();

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var articleDescription = "This is a test article created by E2E tests";
    var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
    var articleTag = "e2etest";

    // Act
    await Pages.EditorPage.CreateArticleWithTagsAsync(articleTitle, articleDescription, articleBody, articleTag);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(articleTitle);
    await Pages.ArticlePage.VerifyAuthorAsync(username);
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    // Arrange - create user and article via API
    var username = GenerateUniqueUsername("editoruser");
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

    await Pages.ArticlePage.ClickEditButtonAsync();

    var updatedTitle = $"{articleTitle} - Updated";

    // Act
    await Pages.EditorPage.UpdateArticleAsync(updatedTitle);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(updatedTitle);
  }
}
