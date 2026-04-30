namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Happy path tests for the Article page (/article/:slug).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "article-happy-001",
    FeatureArea = "articles",
    Behavior = "Tags assigned to an article are displayed on the article detail page",
    Verifies = ["both tags are visible on the article page"])]
  public async Task Tags_AreVisibleOnIndividualArticlePage()
  {
    // Arrange
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token, new[] { tag1, tag2 });

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act
    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Assert
    await Pages.ArticlePage.VerifyArticleTagsVisibleAsync(tag1, tag2);
  }

  [Fact]
  [TestCoverage(
    Id = "article-happy-002",
    FeatureArea = "articles",
    Behavior = "Author can delete their own article and it disappears from the global feed",
    Verifies = ["article is no longer visible in global feed after deletion"])]
  public async Task UserCanDeleteOwnArticle()
  {
    // Arrange - create user and article via API
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act
    await Pages.ArticlePage.DeleteArticleAsync();

    // ToDo: this should actually try to access the article page via its slug and assert a not found error message appears
    // Assert
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleNotVisibleAsync(article.Title);
  }

  [Fact]
  [TestCoverage(
    Id = "article-happy-003",
    FeatureArea = "articles",
    Behavior = "User can favorite and then unfavorite another user's article",
    Verifies = ["favorite button toggles to unfavorite", "unfavorite button toggles back"])]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    // Arrange - create two users in the same tenant
    var user1 = await Api.CreateUserAsync(); // Creates new tenant
    var article = await Api.CreateArticleAsync(user1.Token);

    var user2 = await Api.InviteUserAsync(user1.Token); // Invited to same tenant

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);
    await Expect(Pages.ArticlePage.GetArticleTitle(article.Title)).ToBeVisibleAsync();

    // Act + Assert
    await Pages.ArticlePage.ClickFavoriteButtonAsync();
    await Pages.ArticlePage.ClickUnfavoriteButtonAsync();
  }

  [Fact]
  [TestCoverage(
    Id = "article-happy-004",
    FeatureArea = "articles",
    Behavior = "Authenticated user can add a comment to an article",
    Verifies = ["comment is successfully posted via the article page"])]
  public async Task UserCanAddCommentToArticle()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act + Assert
    var commentText = "This is a test comment from E2E tests!";
    await Pages.ArticlePage.AddCommentAsync(commentText);
  }

  [Fact]
  [TestCoverage(
    Id = "article-happy-005",
    FeatureArea = "articles",
    Behavior = "User can delete their own comment on an article",
    Verifies = ["comment is removed from the article page after deletion"])]
  public async Task UserCanDeleteOwnComment()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    var commentText = "This comment will be deleted!";
    await Api.CreateCommentAsync(user.Token, article.Slug, commentText);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    // Act + Assert
    await Pages.ArticlePage.DeleteCommentAsync(commentText);
  }
}
