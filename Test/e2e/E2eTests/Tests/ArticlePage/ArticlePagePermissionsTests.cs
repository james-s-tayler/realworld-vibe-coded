namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Permission tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class ArticlePagePermissionsTests : AppPageTest
{
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("articleuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticle()
  {
    // Register a user and create an article
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    // Sign out
    await SignOutAsync();

    // Navigate to the article
    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();

    var articlePage = await homePage.ClickArticleAsync(articleTitle);

    // Try to click the favorite button (unauthenticated)
    await articlePage.ClickFavoriteButtonWithoutWaitAsync();

    // Verify redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    // Register a user and create an article
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    // Sign out
    await SignOutAsync();

    // Navigate to the article
    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();

    var articlePage = await homePage.ClickArticleAsync(articleTitle);

    // Click the follow button (unauthenticated)
    await articlePage.ClickFollowButtonWithoutWaitAsync(_testUsername1);

    // Verify redirect to login page
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }
}
