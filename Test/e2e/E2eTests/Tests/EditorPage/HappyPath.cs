namespace E2eTests.Tests.EditorPage;
using static E2eTests.PageModels.Pages;

/// <summary>
/// Happy path tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  [Fact]
  public async Task UserCanCreateArticle_AndViewArticle()
  {
    // Arrange
    await RegisterUserAsync();

    await Pages.HomePage.ClickNewArticleAsync();

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var articleDescription = "This is a test article created by E2E tests";
    var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
    var articleTag = "e2etest";

    // Act
    await Pages.EditorPage.CreateArticleWithTagsAsync(articleTitle, articleDescription, articleBody, articleTag);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(articleTitle);
    await Pages.ArticlePage.VerifyAuthorAsync(TestUsername);
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    // Arrange
    await RegisterUserAsync();
    var articleTitle = await CreateArticleAsync();

    await Pages.ArticlePage.ClickEditButtonAsync();

    var updatedTitle = $"{articleTitle} - Updated";

    // Act
    await Pages.EditorPage.UpdateArticleAsync(updatedTitle);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(updatedTitle);
  }
}
