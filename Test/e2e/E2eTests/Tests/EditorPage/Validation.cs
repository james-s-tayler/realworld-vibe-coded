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
    // Arrange
    var user = await Api.CreateUserAsync();

    var existingArticle = await Api.CreateArticleAsync(user.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(existingArticle.Title, "Different description", "Different body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("has already been taken");
  }

  [Fact]
  public async Task CreateArticle_WithMissingTitle_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync(string.Empty, "Test description", "Test body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("can't be blank");
  }

  [Fact]
  public async Task CreateArticle_WithMissingDescription_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync("Test title", string.Empty, "Test body content");

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("can't be blank");
  }

  [Fact]
  public async Task CreateArticle_WithMissingBody_DisplaysErrorMessage()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.EditorPage.GoToAsync();

    // Act
    await Pages.EditorPage.CreateArticleAndExpectErrorAsync("Test title", "Test description", string.Empty);

    // Assert
    await Pages.EditorPage.VerifyErrorContainsTextAsync("can't be blank");
  }
}
