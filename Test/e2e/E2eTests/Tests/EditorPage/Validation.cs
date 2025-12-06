
namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Validation tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  [Fact]
  public async Task CreateArticle_WithDuplicateTitle_DisplaysErrorMessage()
  {
    // Arrange
    await RegisterUserAsync();

    var timestamp = DateTime.UtcNow.Ticks;
    var articleTitle = $"Duplicate Test Article {timestamp}";

    await Pages.HomePage.ClickNewArticleAsync();

    await Pages.EditorPage.CreateArticleAsync(articleTitle, "Test description", "Test body content");

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(articleTitle, "Different description", "Different body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("has already been taken");
  }
}
