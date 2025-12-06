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
    var (token, username, email, password) = await Api.CreateUserAsync();

    // Create first article to get its title
    var (_, existingTitle) = await Api.CreateArticleAsync(token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    await Pages.EditorPage.GoToAsync();

    // Act - try to create article with the same title
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(existingTitle, "Different description", "Different body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("has already been taken");
  }
}
