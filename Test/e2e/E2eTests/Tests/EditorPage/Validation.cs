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
    var user = await Api.CreateUserAsync();

    // Create first article to get its title
    var existingArticle = await Api.CreateArticleAsync(user.Token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.EditorPage.GoToAsync();

    // Act - try to create article with the same title
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(existingArticle.Title, "Different description", "Different body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("has already been taken");
  }
}
