using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldPaginationE2eTests : ConduitPageTest
{
  [Fact]
  public async Task GlobalFeed_ShowsPaginationWhenMoreThanTenArticles()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Global Feed Pagination Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user
      await RegisterUser();

      // Create 12 articles to trigger pagination (page size is 10)
      for (int i = 1; i <= 12; i++)
      {
        await CreateArticle(i);
      }

      // Navigate to home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify pagination is visible
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify we can navigate to page 2
      var nextPageButton = Page.Locator(".cds--pagination__button--forward");
      await nextPageButton.ClickAsync();

      // Wait for page change
      await Page.WaitForTimeoutAsync(1000);

      // Verify articles are still displayed (page 2 should have articles)
      var articlePreviews = Page.Locator(".article-preview");
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("global_feed_pagination_test");
    }
  }

  [Fact]
  public async Task YourFeed_ShowsPaginationWhenMoreThanTenArticles()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Your Feed Pagination Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create many articles
      var timestamp = DateTime.Now.Ticks;
      var author = $"author{timestamp}";
      var authorEmail = $"author{timestamp}@test.com";
      await RegisterUser(author, authorEmail, TestPassword);

      // Create 12 articles
      for (int i = 1; i <= 12; i++)
      {
        await CreateArticle(i);
      }

      // Sign out
      await SignOut();

      // Register second user
      var follower = $"follower{timestamp}";
      var followerEmail = $"follower{timestamp}@test.com";
      await RegisterUser(follower, followerEmail, TestPassword);

      // Follow first user
      await Page.GotoAsync($"{BaseUrl}/profile/{author}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
      var followButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {author}" });
      await followButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await followButton.ClickAsync();

      // Wait for follow to complete
      await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Unfollow {author}" })).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Navigate to home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Your Feed tab (should be selected by default for logged in users)
      var yourFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Your Feed" });
      await yourFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await yourFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify pagination is visible
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("your_feed_pagination_test");
    }
  }

  private async Task CreateArticle(int index)
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

    var timestamp = DateTime.Now.Ticks;
    var articleTitle = $"Pagination Test Article {index} - {timestamp}";
    var articleDescription = $"Test article {index} for pagination testing";
    var articleBody = $"This is test article {index} body.";

    await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
    await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
    await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
    await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

    await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
  }
}
