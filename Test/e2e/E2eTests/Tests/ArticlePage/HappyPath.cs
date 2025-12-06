namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Happy path tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanDeleteOwnArticle()
  {
    // Arrange - create user and article via API
    var (token, username, email, password) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

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
    // Arrange - create two users and one article via API
    var (user1Token, user1Username, user1Email, user1Password) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(user1Token);

    var (_, user2Username, user2Email, user2Password) = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2Email, user2Password);

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
    // Arrange - create user and article via API
    var (token, username, email, password) = await Api.CreateUserAsync();
    var (_, articleTitle) = await Api.CreateArticleAsync(token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    var commentText = "This is a test comment from E2E tests!";
    await Pages.ArticlePage.AddCommentAsync(commentText);
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    // Arrange - create user, article, and comment via API
    var (token, username, email, password) = await Api.CreateUserAsync();
    var (articleSlug, articleTitle) = await Api.CreateArticleAsync(token);

    var commentText = "This comment will be deleted!";
    await Api.CreateCommentAsync(token, articleSlug, commentText);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(email, password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(articleTitle);

    // Act + Assert
    await Pages.ArticlePage.DeleteCommentAsync(commentText);
  }
}
