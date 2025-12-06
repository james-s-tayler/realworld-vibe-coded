namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Permission tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
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
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    await SignOutAsync();

    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();

    var articlePage = await homePage.ClickArticleAsync(articleTitle);

    // Act
    await articlePage.ClickFavoriteButtonWithoutWaitAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    await SignOutAsync();

    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();
    await homePage.ClickArticleAsync(articleTitle);

    // Act
    await GetArticlePage().ClickFollowButtonWithoutWaitAsync(_testUsername1);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
