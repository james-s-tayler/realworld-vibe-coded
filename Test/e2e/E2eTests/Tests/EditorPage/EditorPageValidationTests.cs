namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Validation tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class EditorPageValidationTests : ConduitPageTest
{
  [Fact]
  public async Task CreateArticle_WithDuplicateTitle_DisplaysErrorMessage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Duplicate Article Title Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user
      await RegisterUserAsync();

      // Create the first article
      var timestamp = DateTime.UtcNow.Ticks;
      var articleTitle = $"Duplicate Test Article {timestamp}";

      var homePage = GetHomePage();
      await homePage.ClickNewArticleAsync();

      var editorPage = GetEditorPage();
      await editorPage.CreateArticleAsync(articleTitle, "Test description", "Test body content");

      // Navigate to create another article with the same title
      await homePage.ClickNewArticleAsync();

      // Fill in the same title and expect error
      await editorPage.CreateArticleAndExpectErrorAsync(articleTitle, "Different description", "Different body content");

      // Verify the error contains the validation message about the slug already being taken
      await editorPage.VerifyErrorContainsTextAsync("has already been taken");
    }
    finally
    {
      await SaveTrace("duplicate_article_title_test");
    }
  }
}
