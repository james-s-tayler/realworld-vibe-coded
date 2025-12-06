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
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(article.Title);

    // Act
    await Pages.ArticlePage.DeleteArticleAsync();

    // ToDo: this should actually try to access the article page via its slug and assert a not found error message appears
    // Assert
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleNotVisibleAsync(article.Title);
  }

  [Fact]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    // Arrange - create user and article via API
    var user1 = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user1.Token);

    // Create a second user and have them view the first user's article
    var user2 = await Api.CreateUserAsync();

    // Log in as user2 via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    // Navigate directly to the article by slug
    await Pages.ArticlePage.GoToAsync(article.Slug);
    
    // Wait for article actions section to be visible (ensures page is loaded and user is authenticated)
    await Expect(Pages.ArticlePage.ArticleActions).ToBeVisibleAsync();

    // Act + Assert
    await Pages.ArticlePage.ClickFavoriteButtonAsync();
    await Pages.ArticlePage.ClickUnfavoriteButtonAsync();
  }

  [Fact]
  public async Task UserCanAddCommentToArticle()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(article.Title);

    // Act + Assert
    var commentText = "This is a test comment from E2E tests!";
    await Pages.ArticlePage.AddCommentAsync(commentText);
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    // Arrange - create user, article, and comment via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    var commentText = "This comment will be deleted!";
    await Api.CreateCommentAsync(user.Token, article.Slug, commentText);

    // Log in via UI
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Navigate to article
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.ClickArticleAsync(article.Title);

    // Act + Assert
    await Pages.ArticlePage.DeleteCommentAsync(commentText);
  }
}
