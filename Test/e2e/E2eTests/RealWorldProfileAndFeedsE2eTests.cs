using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldProfileAndFeedsE2eTests : PageTest
{
  private const int DefaultTimeout = 30000;
  private string _baseUrl = null!;
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;
  private string _testUsername2 = null!;
  private string _testEmail2 = null!;
  private string _testPassword2 = null!;

  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true,
    };
  }

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    var timestamp = DateTime.Now.Ticks;
    _testUsername1 = $"profileuser1_{timestamp}";
    _testEmail1 = $"profileuser1_{timestamp}@test.com";
    _testPassword1 = "TestPassword123!";

    _testUsername2 = $"profileuser2_{timestamp}";
    _testEmail2 = $"profileuser2_{timestamp}@test.com";
    _testPassword2 = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  [Fact]
  public async Task UserCanViewOtherUsersProfile()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "View Other User Profile Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticle(_testUsername1);

      // Sign out
      await SignOut();

      // Register second user
      await RegisterUser(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to home and find first user's article
      await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on first user's name/profile link from an article
      var authorLink = Page.GetByRole(AriaRole.Link, new() { Name = _testUsername1 }).First;
      await authorLink.WaitForAsync(new() { Timeout = DefaultTimeout });
      await authorLink.ClickAsync();

      // Verify on profile page
      await Page.WaitForURLAsync($"{_baseUrl}/profile/{_testUsername1}", new() { Timeout = DefaultTimeout });

      // Verify profile information is displayed
      var profileHeading = Page.GetByRole(AriaRole.Heading, new() { Name = _testUsername1 });
      await profileHeading.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await profileHeading.IsVisibleAsync(), "Other user's profile should be visible");

      // Verify tabs are present
      var myPostsTab = Page.Locator("button[role='tab']").Filter(new() { HasText = "My Articles" }).Or(Page.Locator("button[role='tab']").Filter(new() { HasText = "My Posts" }));
      Assert.True(await myPostsTab.First.IsVisibleAsync(), "My Posts tab should be visible on other user's profile");
    }
    finally
    {
      await SaveTrace("view_other_profile_test");
    }
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Follow User Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticle(_testUsername1);

      // Sign out
      await SignOut();

      // Register second user
      await RegisterUser(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to first user's profile
      await Page.GotoAsync($"{_baseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Find and click follow button
      var followButton = Page.Locator("button").Filter(new() { HasText = "Follow" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();

      // Wait for update
      await Page.WaitForTimeoutAsync(1000);

      // Verify button text changed to unfollow
      var unfollowButton = Page.Locator("button").Filter(new() { HasText = "Unfollow" });
      Assert.True(await unfollowButton.IsVisibleAsync(), "Unfollow button should appear after following");

      // Click to unfollow
      await unfollowButton.ClickAsync();

      // Wait for update
      await Page.WaitForTimeoutAsync(1000);

      // Verify button text changed back to follow
      followButton = Page.Locator("button").Filter(new() { HasText = "Follow" });
      Assert.True(await followButton.IsVisibleAsync(), "Follow button should appear after unfollowing");
    }
    finally
    {
      await SaveTrace("follow_user_test");
    }
  }

  [Fact]
  public async Task YourFeed_ShowsArticlesFromFollowedUsers()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Your Feed Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var articleTitle = await CreateArticle(_testUsername1);

      // Sign out
      await SignOut();

      // Register second user
      await RegisterUser(_testUsername2, _testEmail2, _testPassword2);

      // Follow first user
      await Page.GotoAsync($"{_baseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
      var followButton = Page.Locator("button").Filter(new() { HasText = "Follow" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Navigate to home page
      await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Your Feed tab
      var yourFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Your Feed" });
      await yourFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await yourFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify followed user's article appears in Your Feed
      var article = Page.Locator($"text=/{articleTitle}/i").First;
      await article.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await article.IsVisibleAsync(), "Followed user's article should appear in Your Feed");
    }
    finally
    {
      await SaveTrace("your_feed_test");
    }
  }

  [Fact]
  public async Task UserCanFilterArticlesByTag()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Filter by Tag Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article with a specific tag
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var testTag = $"testtag{DateTime.Now.Ticks}";
      var articleTitle = await CreateArticleWithTag(testTag);

      // Navigate to home page
      await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Wait for tags to load
      await Page.WaitForTimeoutAsync(2000);

      // Click on the tag in popular tags (or use the tag from the article)
      var tagLink = Page.Locator($"text=/{testTag}/i").First;
      await tagLink.WaitForAsync(new() { Timeout = DefaultTimeout });
      await tagLink.ClickAsync();

      // Wait for filtered results
      await Page.WaitForTimeoutAsync(2000);

      // Verify we're on a tag filter view (tab should show the tag name)
      var tagTab = Page.Locator($"button[role='tab']:has-text('{testTag}')").First;
      Assert.True(await tagTab.IsVisibleAsync(), "Tag tab should be visible when filtering by tag");

      // Verify the article with that tag is displayed
      var article = Page.GetByText(articleTitle).First;
      Assert.True(await article.IsVisibleAsync(), "Article with the selected tag should be visible");
    }
    finally
    {
      await SaveTrace("filter_by_tag_test");
    }
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Favorited Articles on Profile Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var articleTitle = await CreateArticle(_testUsername1);

      // Favorite the article
      var favoriteButton = Page.Locator("button:has-text('Favorite')").First;
      await favoriteButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await favoriteButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Navigate to own profile
      await Page.GetByRole(AriaRole.Link, new() { Name = _testUsername1 }).First.ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/profile/{_testUsername1}", new() { Timeout = DefaultTimeout });

      // Click on Favorited Posts tab
      var favoritedTab = Page.Locator("button[role='tab']").Filter(new() { HasText = "Favorited" });
      await favoritedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await favoritedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify favorited article appears
      var article = Page.Locator($"text=/{articleTitle}/i").First;
      Assert.True(await article.IsVisibleAsync(), "Favorited article should appear in Favorited Posts tab");
    }
    finally
    {
      await SaveTrace("favorited_articles_profile_test");
    }
  }

  // Helper methods
  private async Task RegisterUser(string username, string email, string password)
  {
    await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/register", new() { Timeout = DefaultTimeout });

    await Page.GetByPlaceholder("Username").FillAsync(username);
    await Page.GetByPlaceholder("Email").FillAsync(email);
    await Page.GetByPlaceholder("Password").FillAsync(password);

    // Click submit and wait for API response and navigation
    var responseTask = Page.WaitForResponseAsync(
      response =>
      response.Url.Contains("/api/users") && response.Request.Method == "POST",
      new() { Timeout = DefaultTimeout });

    await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

    await responseTask;

    // Wait for the user link to appear in the header to confirm login and navigation completed
    await Page.GetByRole(AriaRole.Link, new() { Name = username }).First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
  }

  private async Task SignOut()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/settings", new() { Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
    await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });
  }

  private async Task<string> CreateArticle(string username)
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/editor", new() { Timeout = DefaultTimeout });

    var timestamp = DateTime.Now.Ticks;
    var articleTitle = $"{username} Article {timestamp}";
    var articleDescription = "Test article";
    var articleBody = "This is a test article body.";

    await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
    await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
    await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
    await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

    await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
    return articleTitle;
  }

  private async Task<string> CreateArticleWithTag(string tag)
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/editor", new() { Timeout = DefaultTimeout });

    var timestamp = DateTime.Now.Ticks;
    var articleTitle = $"Tagged Article {timestamp}";
    var articleDescription = "Test article with tag";
    var articleBody = "This is a test article body.";

    await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
    await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
    await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
    await Page.GetByPlaceholder("Enter tags").FillAsync(tag);
    await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

    await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
    return articleTitle;
  }

  private async Task SaveTrace(string testName)
  {
    if (!Directory.Exists(Constants.TracesDirectory))
    {
      Directory.CreateDirectory(Constants.TracesDirectory);
    }

    await Context.Tracing.StopAsync(new()
    {
      Path = Path.Combine(Constants.TracesDirectory, $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
    });
  }
}
