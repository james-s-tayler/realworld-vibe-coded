using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldProfileAndFeedsE2eTests : ConduitPageTest
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

    var timestamp = DateTime.Now.Ticks;
    _testUsername1 = $"profileuser1_{timestamp}";
    _testEmail1 = $"profileuser1_{timestamp}@test.com";
    _testPassword1 = "TestPassword123!";

    _testUsername2 = $"profileuser2_{timestamp}";
    _testEmail2 = $"profileuser2_{timestamp}@test.com";
    _testPassword2 = "TestPassword123!";
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

      // Navigate directly to first user's profile
      await Page.GotoAsync($"{BaseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Verify profile information is displayed - use Exact match to avoid matching article title
      var profileHeading = Page.GetByRole(AriaRole.Heading, new() { Name = _testUsername1, Exact = true });
      await profileHeading.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await profileHeading.IsVisibleAsync(), "Other user's profile should be visible");

      // Verify tabs are present
      var myPostsTab = Page.GetByRole(AriaRole.Tab, new() { Name = "My Articles" });
      Assert.True(await myPostsTab.IsVisibleAsync(), "My Articles tab should be visible on other user's profile");
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
      await Page.GotoAsync($"{BaseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Find and click follow button - The button text is "Follow {username}"
      var followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {_testUsername1}" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();

      // Wait for update
      await Page.WaitForTimeoutAsync(1000);

      // Verify button text changed to unfollow
      var unfollowButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Unfollow {_testUsername1}" });
      await Expect(unfollowButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click to unfollow
      await unfollowButton.ClickAsync();

      // Wait for update
      await Page.WaitForTimeoutAsync(1000);

      // Verify button text changed back to follow
      followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {_testUsername1}" });
      await Expect(followButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
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
      await Page.GotoAsync($"{BaseUrl}/profile/{_testUsername1}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
      var followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {_testUsername1}" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();

      // Wait for follow to complete
      await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Unfollow {_testUsername1}" })).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Navigate to home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Your Feed tab
      var yourFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Your Feed" });
      await yourFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await yourFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify followed user's article appears in Your Feed
      var article = Page.GetByText(articleTitle).First;
      await Expect(article).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
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
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // First go to Global Feed to see the article
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Find and click on the tag in the article preview - it's a Tag component
      var tagElement = Page.Locator($".cds--tag:has-text('{testTag}')").First;
      await tagElement.ScrollIntoViewIfNeededAsync();
      await tagElement.ClickAsync();

      // Wait for filtered results
      await Page.WaitForTimeoutAsync(2000);

      // Verify we're on a tag filter view (tab should show the tag name with #)
      var tagTab = Page.GetByRole(AriaRole.Tab, new() { Name = $"#{testTag}" });
      await Expect(tagTab).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

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
      // Register first user and create an article
      await RegisterUser(_testUsername1, _testEmail1, _testPassword1);
      var articleTitle = await CreateArticle(_testUsername1);

      // Sign out
      await SignOut();

      // Register second user
      await RegisterUser(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to the first user's article
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.ClickAsync();
      await Page.WaitForTimeoutAsync(2000);

      // Click on the article
      await Page.GetByText(articleTitle).First.ClickAsync();
      await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Favorite the article - button text contains "Favorite Article"
      var favoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Favorite Article") });
      await favoriteButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await favoriteButton.ClickAsync();

      // Wait for unfavorite button to confirm action completed
      var unfavoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Unfavorite Article") });
      await Expect(unfavoriteButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Navigate to own profile
      await Page.GetByRole(AriaRole.Link, new() { Name = _testUsername2 }).First.ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/profile/{_testUsername2}", new() { Timeout = DefaultTimeout });

      // Click on Favorited Articles tab
      var favoritedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Favorited Articles" });
      await favoritedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await favoritedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify favorited article appears
      var article = Page.GetByText(articleTitle).First;
      await Expect(article).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("favorited_articles_profile_test");
    }
  }

  private async Task<string> CreateArticle(string username)
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

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
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

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
}
