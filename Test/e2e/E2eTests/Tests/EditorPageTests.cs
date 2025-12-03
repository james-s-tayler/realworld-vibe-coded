namespace E2eTests.Tests;

/// <summary>
/// Tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class EditorPageTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanCreateArticle_AndViewArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Create and View Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, sign up a user
      await RegisterUserAsync();

      // Navigate to new article page using page model
      var homePage = GetHomePage();
      await homePage.ClickNewArticleAsync();

      // Fill in article form using page model
      var editorPage = GetEditorPage();
      var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
      var articleDescription = "This is a test article created by E2E tests";
      var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
      var articleTag = "e2etest";

      var articlePage = await editorPage.CreateArticleWithTagsAsync(articleTitle, articleDescription, articleBody, articleTag);

      // Verify article content
      await articlePage.VerifyArticleTitleAsync(articleTitle);
      await articlePage.VerifyAuthorAsync(TestUsername);
    }
    finally
    {
      await SaveTrace("create_article_test");
    }
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Edit Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUserAsync();
      var (articlePage, articleTitle) = await CreateArticleAsync();

      // Click edit button on article page using page model
      var editorPage = await articlePage.ClickEditButtonAsync();

      // Update article title
      var updatedTitle = $"{articleTitle} - Updated";
      var updatedArticlePage = await editorPage.UpdateArticleAsync(updatedTitle);

      // Verify updated title is displayed
      await updatedArticlePage.VerifyArticleTitleAsync(updatedTitle);
    }
    finally
    {
      await SaveTrace("edit_article_test");
    }
  }

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
