namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Happy path tests for the Profile page (/profile/:username).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  private const int TotalArticles = 50;

  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanViewOtherUsersProfile()
  {
    // Arrange - create two users and article via API
    var (user1Token, user1Username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user1Token);

    var (_, _, user2Email, user2Password) = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

    // Act
    await Pages.ProfilePage.GoToAsync(user1Username);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(user1Username);
    await Pages.ProfilePage.VerifyMyArticlesTabVisibleAsync();
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    // Arrange - create two users and article via API
    var (user1Token, user1Username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user1Token);

    var (_, _, user2Email, user2Password) = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

    await Pages.ProfilePage.GoToAsync(user1Username);

    // Act + Assert
    await Pages.ProfilePage.ClickFollowButtonAsync(user1Username);
    await Pages.ProfilePage.ClickUnfollowButtonAsync(user1Username);
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    // Arrange - create two users, article, and favorite via API
    var (user1Token, _, _, _) = await Api.CreateUserAsync();
    var (articleSlug, articleTitle) = await Api.CreateArticleAsync(user1Token);

    var (user2Token, user2Username, user2Email, user2Password) = await Api.CreateUserAsync();
    await Api.FavoriteArticleAsync(user2Token, articleSlug);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

    await Pages.ProfilePage.GoToAsync(user2Username);

    // Act
    await Pages.ProfilePage.ClickFavoritedArticlesTabAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task ProfilePage_MyArticles_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange - create user and articles via API
    var (token, username, _, _) = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(token, TotalArticles);

    await Pages.ProfilePage.GoToAsync(username);
    await Pages.ProfilePage.WaitForArticlesToLoadAsync();

    // Act
    await Pages.ProfilePage.VerifyArticleCountAsync(20);
    await Pages.ProfilePage.VerifyPaginationVisibleAsync();
    await Pages.ProfilePage.ClickNextPageAsync();
    await Pages.ProfilePage.ClickNextPageAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleCountAsync(10);
    await Pages.ProfilePage.ClickPreviousPageAsync();
    await Pages.ProfilePage.VerifyArticleCountAsync(20);
  }
}
