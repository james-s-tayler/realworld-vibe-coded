using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class UnauthenticatedUserRedirectE2eTests : ConduitPageTest
{
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("unauthuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";
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
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var articleTitle = await CreateArticle();

      // Sign out
      await SignOut();

      // Navigate to the article
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.ClickAsync();

      // Wait for article preview to be visible
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreview = visiblePanel.Locator(".article-preview").Filter(new() { HasText = articleTitle }).First;
      await Expect(articlePreview).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click on the article link
      var articleLink = articlePreview.Locator(".article-link");
      await articleLink.ClickAsync();
      await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Try to click the favorite button - button text contains "Favorite Article"
      var favoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("Favorite Article") });
      await favoriteButton.ClickAsync();

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
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticleFromHomePage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Favorite From Home Page Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticle();

      // Sign out
      await SignOut();

      // Navigate to the home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.ClickAsync();

      // Wait for and click the favorite button on the article preview
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var favoriteButton = visiblePanel.Locator(".article-preview .favorite-button").First;
      await Expect(favoriteButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      await favoriteButton.ClickAsync();

      // Verify redirect to login page
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_favorite_home_page_redirect_test");
    }
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Follow User Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticle();

      // Sign out
      await SignOut();

      // Navigate to the user's profile
      await Page.GotoAsync($"{BaseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Wait for profile to load
      var profileHeading = Page.GetByRole(AriaRole.Heading, new() { Name = _testUsername1, Exact = true });
      await profileHeading.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Find and click the follow button
      var followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {_testUsername1}" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();

      // Verify redirect to login page
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_follow_redirect_test");
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
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var articleTitle = await CreateArticle();

      // Sign out
      await SignOut();

      // Navigate to the article
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.ClickAsync();

      // Wait for article preview to be visible
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreview = visiblePanel.Locator(".article-preview").Filter(new() { HasText = articleTitle }).First;
      await Expect(articlePreview).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click on the article link
      var articleLink = articlePreview.Locator(".article-link");
      await articleLink.ClickAsync();
      await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Find and click the follow button
      var followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {_testUsername1}" });
      await followButton.ClickAsync();

      // Verify redirect to login page
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_follow_article_page_redirect_test");
    }
  }

  private async Task<string> CreateArticle()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("article")}";
    var articleDescription = "Test article for E2E testing";
    var articleBody = "This is a test article body.";

    await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
    await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
    await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
    await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

    await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = DefaultTimeout });
    var articleHeading = Page.GetByRole(AriaRole.Heading, new() { Name = articleTitle });
    await Expect(articleHeading).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

    return articleTitle;
  }
}
