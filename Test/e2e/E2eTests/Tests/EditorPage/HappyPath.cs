namespace E2eTests.Tests.EditorPage;

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

    var homePage = GetHomePage();
    await homePage.ClickNewArticleAsync();

    var editorPage = GetEditorPage();
    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var articleDescription = "This is a test article created by E2E tests";
    var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
    var articleTag = "e2etest";

    // Act
    var articlePage = await editorPage.CreateArticleWithTagsAsync(articleTitle, articleDescription, articleBody, articleTag);

    // Assert
    await articlePage.VerifyArticleTitleAsync(articleTitle);
    await articlePage.VerifyAuthorAsync(TestUsername);
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    // Arrange
    await RegisterUserAsync();
    var (articlePage, articleTitle) = await CreateArticleAsync();

    var editorPage = await articlePage.ClickEditButtonAsync();

    var updatedTitle = $"{articleTitle} - Updated";

    // Act
    var updatedArticlePage = await editorPage.UpdateArticleAsync(updatedTitle);

    // Assert
    await updatedArticlePage.VerifyArticleTitleAsync(updatedTitle);
  }
}
