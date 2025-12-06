namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Validation tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class EditorPageValidationTests : AppPageTest
{
  [Fact]
  public async Task CreateArticle_WithDuplicateTitle_DisplaysErrorMessage()
  {
    // Arrange
    await RegisterUserAsync();

    var timestamp = DateTime.UtcNow.Ticks;
    var articleTitle = $"Duplicate Test Article {timestamp}";

    var homePage = GetHomePage();
    await homePage.ClickNewArticleAsync();

    var editorPage = GetEditorPage();
    await editorPage.CreateArticleAsync(articleTitle, "Test description", "Test body content");

    await homePage.ClickNewArticleAsync();

    // Act
    await editorPage.CreateArticleAndExpectErrorAsync(articleTitle, "Different description", "Different body content");

    // Assert
    await editorPage.VerifyErrorContainsTextAsync("has already been taken");
  }
}
