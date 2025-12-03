namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Happy path tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class ArticlePageHappyPathTests : ConduitPageTest
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
    // Register user and create an article
    await RegisterUserAsync();
    var (articlePage, articleTitle) = await CreateArticleAsync();

    // Delete article using page model
    var homePage = await articlePage.DeleteArticleAsync();

    // Check that the deleted article is not in the feed
    await homePage.ClickGlobalFeedTabAsync();
    await homePage.VerifyArticleNotVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    // Register first user and create an article
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    // Sign out
    await SignOutAsync();

    // Register second user
    await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

    // Navigate to the first user's article
    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();

    var articlePage = await homePage.ClickArticleAsync(articleTitle);

    // Favorite and unfavorite using page model
    await articlePage.ClickFavoriteButtonAsync();
    await articlePage.ClickUnfavoriteButtonAsync();
  }

  [Fact]
  public async Task UserCanAddCommentToArticle()
  {
    // Register user and create an article
    await RegisterUserAsync();
    var (articlePage, _) = await CreateArticleAsync();

    // Add a comment using page model
    var commentText = "This is a test comment from E2E tests!";
    await articlePage.AddCommentAsync(commentText);

    // Verify comment is displayed
    await articlePage.VerifyCommentVisibleAsync(commentText);
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    // Register user and create an article
    await RegisterUserAsync();
    var (articlePage, _) = await CreateArticleAsync();

    // Add a comment
    var commentText = "This comment will be deleted!";
    await articlePage.AddCommentAsync(commentText);

    // Delete the comment using page model
    await articlePage.DeleteCommentAsync(commentText);

    // Verify comment is no longer visible
    await articlePage.VerifyCommentNotVisibleAsync(commentText);
  }
}
