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
    var user1 = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    // Act
    await Pages.ProfilePage.GoToAsync(user1.Username);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(user1.Username);
    await Pages.ProfilePage.VerifyMyArticlesTabVisibleAsync();
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    // Arrange - create two users and article via API
    var user1 = await Api.CreateUserAsync();
    await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    await Pages.ProfilePage.GoToAsync(user1.Username);

    // Act + Assert
    await Pages.ProfilePage.ClickFollowButtonAsync(user1.Username);
    await Pages.ProfilePage.ClickUnfollowButtonAsync(user1.Username);
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    // Arrange - create two users, article, and favorite via API
    var user1 = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.CreateUserAsync();
    await Api.FavoriteArticleAsync(user2.Token, article.Slug);

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    await Pages.ProfilePage.GoToAsync(user2.Username);

    // Act
    await Pages.ProfilePage.ClickFavoritedArticlesTabAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  public async Task ProfilePage_MyArticles_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange - create user and articles via API
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, TotalArticles);

    await Pages.ProfilePage.GoToAsync(user.Username);
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
