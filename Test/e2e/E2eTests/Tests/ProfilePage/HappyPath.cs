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
    // Arrange
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    // Act
    await Pages.ProfilePage.GoToAsync(user1.Email);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(user1.Email);
    await Pages.ProfilePage.VerifyMyArticlesTabVisibleAsync();
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    await Pages.ProfilePage.GoToAsync(user1.Email);

    // Act + Assert
    await Pages.ProfilePage.ClickFollowButtonAsync(user1.Email);
    await Pages.ProfilePage.ClickUnfollowButtonAsync(user1.Email);
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    var article = await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant
    await Api.FavoriteArticleAsync(user2.Token, article.Slug);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    await Pages.ProfilePage.GoToAsync(user2.Email);

    // Act
    await Pages.ProfilePage.ClickFavoritedArticlesTabAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleVisibleAsync(article.Title);
  }

  [Fact]
  public async Task ProfilePage_MyArticles_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Api.CreateArticlesAsync(user.Token, TotalArticles);

    // Log in to access the profile page (now requires authentication)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ProfilePage.GoToAsync(user.Email);
    await Pages.ProfilePage.WaitForArticlesToLoadAsync();
    await Pages.ProfilePage.VerifyArticleCountAsync(20);
    await Pages.ProfilePage.VerifyPaginationVisibleAsync();

    // Act
    await Pages.ProfilePage.ClickNextPageAsync();
    await Pages.ProfilePage.ClickNextPageAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleCountAsync(10);
    await Pages.ProfilePage.ClickPreviousPageAsync();
    await Pages.ProfilePage.VerifyArticleCountAsync(20);
  }
}
