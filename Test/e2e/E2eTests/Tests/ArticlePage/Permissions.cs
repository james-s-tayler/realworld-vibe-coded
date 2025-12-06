
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
    var articleTitle = await CreateArticleAsync();

    await SignOutAsync();

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act
    await Pages.ArticlePage.ClickFavoriteButtonWithoutWaitAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var articleTitle = await CreateArticleAsync();

    await SignOutAsync();

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act
    await Pages.ArticlePage.ClickFollowButtonWithoutWaitAsync(_testUsername1);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
  }
}
