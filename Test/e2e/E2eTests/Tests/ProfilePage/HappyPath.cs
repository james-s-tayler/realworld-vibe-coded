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
    var user1Username = GenerateUniqueUsername("profileuser1");
    var user1Email = GenerateUniqueEmail(user1Username);
    var user1Password = "TestPassword123!";
    var (user1Token, _) = await Api.CreateUserAsync(user1Username, user1Email, user1Password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    await Api.CreateArticleAsync(
      user1Token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var user2Username = GenerateUniqueUsername("profileuser2");
    var user2Email = GenerateUniqueEmail(user2Username);
    var user2Password = "TestPassword123!";
    var (user2Token, _) = await Api.CreateUserAsync(user2Username, user2Email, user2Password);

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
    var user1Username = GenerateUniqueUsername("profileuser1");
    var user1Email = GenerateUniqueEmail(user1Username);
    var user1Password = "TestPassword123!";
    var (user1Token, _) = await Api.CreateUserAsync(user1Username, user1Email, user1Password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    await Api.CreateArticleAsync(
      user1Token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var user2Username = GenerateUniqueUsername("profileuser2");
    var user2Email = GenerateUniqueEmail(user2Username);
    var user2Password = "TestPassword123!";
    var (user2Token, _) = await Api.CreateUserAsync(user2Username, user2Email, user2Password);

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
    var user1Username = GenerateUniqueUsername("profileuser1");
    var user1Email = GenerateUniqueEmail(user1Username);
    var user1Password = "TestPassword123!";
    var (user1Token, _) = await Api.CreateUserAsync(user1Username, user1Email, user1Password);

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var (articleSlug, _) = await Api.CreateArticleAsync(
      user1Token,
      articleTitle,
      "Test article for E2E testing",
      "This is a test article body.");

    var user2Username = GenerateUniqueUsername("profileuser2");
    var user2Email = GenerateUniqueEmail(user2Username);
    var user2Password = "TestPassword123!";
    var (user2Token, _) = await Api.CreateUserAsync(user2Username, user2Email, user2Password);

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
    var uniqueId = GenerateUniqueUsername("profileuser");
    var email = $"{uniqueId}@test.com";
    var (token, username) = await Api.CreateUserAsync(uniqueId, email, "TestPassword123!");
    await Api.CreateArticlesAsync(token, TotalArticles, uniqueId);

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
