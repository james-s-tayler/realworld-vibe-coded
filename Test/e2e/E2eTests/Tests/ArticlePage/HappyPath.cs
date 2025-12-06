
namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Happy path tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;
  private string _testUsername2 = null!;
  private string _testEmail2 = null!;
  private string _testPassword2 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("articleuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";

    _testUsername2 = GenerateUniqueUsername("articleuser2");
    _testEmail2 = GenerateUniqueEmail(_testUsername2);
    _testPassword2 = "TestPassword123!";
  }

  [Fact]
  public async Task UserCanDeleteOwnArticle()
  {
    // Arrange
    await RegisterUserAsync();
    var articleTitle = await CreateArticleAsync();

    // Act
    await Pages.ArticlePage.DeleteArticleAsync();

    // ToDo: this should actually try to access the article page via its slug and assert a not found error message appears
    // Assert
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleNotVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var articleTitle = await CreateArticleAsync();

    await SignOutAsync();

    await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    await Pages.ArticlePage.ClickFavoriteButtonAsync();
    await Pages.ArticlePage.ClickUnfavoriteButtonAsync();
  }

  [Fact]
  public async Task UserCanAddCommentToArticle()
  {
    // Arrange
    await RegisterUserAsync();
    await CreateArticleAsync();

    // Act + Assert
    var commentText = "This is a test comment from E2E tests!";
    await Pages.ArticlePage.AddCommentAsync(commentText);
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    // Arrange
    await RegisterUserAsync();
    await CreateArticleAsync();

    var commentText = "This comment will be deleted!";
    await Pages.ArticlePage.AddCommentAsync(commentText);

    // Act + Assert
    await Pages.ArticlePage.DeleteCommentAsync(commentText);
  }
}
