namespace E2eTests.Tests;

/// <summary>
/// Tests for the Article page (/article/:slug) including comments, favorites, and deletion.
/// </summary>
[Collection("E2E Tests")]
public class ArticlePageTests : ConduitPageTest
{
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;
  private string _testUsername2 = null!;
  private string _testEmail2 = null!;
  private string _testPassword2 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("articleuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";

    _testUsername2 = GenerateUniqueUsername("articleuser2");
    _testEmail2 = GenerateUniqueEmail(_testUsername2);
    _testPassword2 = "TestPassword123!";
  }

  [Fact]
  public async Task UserCanDeleteOwnArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Delete Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUserAsync();
      var (articlePage, articleTitle) = await CreateArticleAsync();

      // Delete article using page model
      var homePage = await articlePage.DeleteArticleAsync();

      // Check that the deleted article is not in the feed
      await homePage.ClickGlobalFeedTabAsync();
      await homePage.VerifyArticleNotVisibleAsync(articleTitle);
    }
    finally
    {
      await SaveTrace("delete_article_test");
    }
  }

  [Fact]
  public async Task UserCanFavoriteAndUnfavoriteArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Favorite Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      var (_, articleTitle) = await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Register second user
      await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to the first user's article
      var homePage = GetHomePage();
      await homePage.GoToAsync();
      await homePage.ClickGlobalFeedTabAsync();

      var articlePage = await homePage.ClickArticleAsync(articleTitle);

      // Favorite and unfavorite using page model
      await articlePage.ClickFavoriteButtonAsync();
      await articlePage.ClickUnfavoriteButtonAsync();
    }
    finally
    {
      await SaveTrace("favorite_article_test");
    }
  }

  [Fact]
  public async Task UserCanAddCommentToArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Add Comment Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUserAsync();
      var (articlePage, _) = await CreateArticleAsync();

      // Add a comment using page model
      var commentText = "This is a test comment from E2E tests!";
      await articlePage.AddCommentAsync(commentText);

      // Verify comment is displayed
      await articlePage.VerifyCommentVisibleAsync(commentText);
    }
    finally
    {
      await SaveTrace("add_comment_test");
    }
  }

  [Fact]
  public async Task UserCanDeleteOwnComment()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Delete Comment Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUserAsync();
      var (articlePage, _) = await CreateArticleAsync();

      // Add a comment
      var commentText = "This comment will be deleted!";
      await articlePage.AddCommentAsync(commentText);

      // Delete the comment using page model
      await articlePage.DeleteCommentAsync(commentText);

      // Verify comment is no longer visible
      await articlePage.VerifyCommentNotVisibleAsync(commentText);
    }
    finally
    {
      await SaveTrace("delete_comment_test");
    }
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Favorite Article Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
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
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_favorite_redirect_test");
    }
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUserFromArticlePage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Follow From Article Page Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
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
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_follow_article_page_redirect_test");
    }
  }
}
