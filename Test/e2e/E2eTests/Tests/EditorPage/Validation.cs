namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Validation tests for the Editor page (/editor and /editor/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task CreateArticle_WithDuplicateTitle_DisplaysErrorMessage()
  {
    // Arrange - create user and article via API
    var username = GenerateUniqueUsername("editoruser");
    var email = GenerateUniqueEmail(username);
    var password = "TestPassword123!";
    var (token, _) = await Api.CreateUserAsync(username, email, password);

    var timestamp = DateTime.UtcNow.Ticks;
    var articleTitle = $"Duplicate Test Article {timestamp}";
    await Api.CreateArticleAsync(token, articleTitle, "Test description", "Test body content");

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(articleTitle, "Different description", "Different body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("has already been taken");
  }
}
