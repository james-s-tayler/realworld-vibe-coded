using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using System.Text.RegularExpressions;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldArticleInteractionsE2eTests : PageTest
{
  private const int DefaultTimeout = 30000;
  private string _baseUrl = null!;
  private string _testUsername = null!;
  private string _testEmail = null!;
  private string _testPassword = null!;

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
    _testUsername = $"articleuser{timestamp}";
    _testEmail = $"articleuser{timestamp}@test.com";
    _testPassword = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Edit Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register user and create an article
      await RegisterUser();
      var articleTitle = await CreateArticle();

      // Click edit button on article page
      var editButton = Page.GetByRole(AriaRole.Button, new() { Name = /edit article/i });
      await editButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await editButton.ClickAsync();

      // Wait for editor page with slug
      await Page.WaitForURLAsync(new Regex(@"/editor/"), new() { Timeout = DefaultTimeout });

      // Update article title
      var updatedTitle = $"{articleTitle} - Updated";
      var titleInput = Page.GetByPlaceholder("Article Title");
      await titleInput.ClearAsync();
      await titleInput.FillAsync(updatedTitle);

      // Submit update
      await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

      // Verify redirect to article page
      await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Verify updated title is displayed
      var updatedHeading = Page.GetByRole(AriaRole.Heading, new() { Name = updatedTitle });
      await updatedHeading.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await updatedHeading.IsVisibleAsync(), "Updated article title should be displayed");
    }
    finally
    {
      await SaveTrace("edit_article_test");
    }
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
      await RegisterUser();
      var articleTitle = await CreateArticle();

      // Click delete button on article page
      var deleteButton = Page.GetByRole(AriaRole.Button, new() { Name = /delete article/i });
      await deleteButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await deleteButton.ClickAsync();

      // Verify redirect to home page after deletion
      await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });

      // Verify article no longer appears in feed (give it time to load)
      await Page.WaitForTimeoutAsync(2000);

      // Check that the deleted article is not in the feed
      var deletedArticle = Page.Locator($"text=/{articleTitle}/i").First;
      var isVisible = await deletedArticle.IsVisibleAsync();
      Assert.False(isVisible, "Deleted article should not appear in feed");
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
      // Register user and create an article
      await RegisterUser();
      await CreateArticle();

      // Find favorite button (may have different text/icon)
      var favoriteButton = Page.Locator("button").Filter(new() { HasText = new Regex("favorite", RegexOptions.IgnoreCase) }).First;
      await favoriteButton.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Get initial favorite count
      var initialText = await favoriteButton.TextContentAsync() ?? "";

      // Click to favorite
      await favoriteButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000); // Wait for update

      // Verify favorite count increased
      var updatedText = await favoriteButton.TextContentAsync() ?? "";
      Assert.NotEqual(initialText, updatedText);

      // Click to unfavorite
      await favoriteButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000); // Wait for update

      // Verify favorite count decreased back
      var finalText = await favoriteButton.TextContentAsync() ?? "";
      Assert.Equal(initialText, finalText);
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
      await RegisterUser();
      await CreateArticle();

      // Add a comment
      var commentText = "This is a test comment from E2E tests!";
      var commentTextarea = Page.GetByPlaceholder(/write a comment/i);
      await commentTextarea.WaitForAsync(new() { Timeout = DefaultTimeout });
      await commentTextarea.FillAsync(commentText);

      var postButton = Page.GetByRole(AriaRole.Button, new() { Name = /post comment/i });
      await postButton.ClickAsync();

      // Wait for comment to appear
      await Page.WaitForTimeoutAsync(2000);

      // Verify comment is displayed
      var comment = Page.Locator($"text=/{commentText}/i");
      await comment.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await comment.IsVisibleAsync(), "Posted comment should be visible");
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
      await RegisterUser();
      await CreateArticle();

      // Add a comment
      var commentText = "This comment will be deleted!";
      var commentTextarea = Page.GetByPlaceholder(/write a comment/i);
      await commentTextarea.WaitForAsync(new() { Timeout = DefaultTimeout });
      await commentTextarea.FillAsync(commentText);

      var postButton = Page.GetByRole(AriaRole.Button, new() { Name = /post comment/i });
      await postButton.ClickAsync();

      // Wait for comment to appear
      await Page.WaitForTimeoutAsync(2000);

      // Find and click delete button on the comment
      var deleteButton = Page.GetByRole(AriaRole.Button, new() { Name = /delete/i }).First;
      await deleteButton.WaitForAsync(new() { Timeout = DefaultTimeout });
      await deleteButton.ClickAsync();

      // Wait for deletion
      await Page.WaitForTimeoutAsync(2000);

      // Verify comment is no longer visible
      var deletedComment = Page.Locator($"text=/{commentText}/i");
      Assert.False(await deletedComment.IsVisibleAsync(), "Deleted comment should not be visible");
    }
    finally
    {
      await SaveTrace("delete_comment_test");
    }
  }

  // Helper methods
  private async Task RegisterUser()
  {
    await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/register", new() { Timeout = DefaultTimeout });

    await Page.GetByPlaceholder("Username").FillAsync(_testUsername);
    await Page.GetByPlaceholder("Email").FillAsync(_testEmail);
    await Page.GetByPlaceholder("Password").FillAsync(_testPassword);
    await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

    await Page.WaitForURLAsync(_baseUrl, new() { Timeout = DefaultTimeout });
  }

  private async Task<string> CreateArticle()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/editor", new() { Timeout = DefaultTimeout });

    var timestamp = DateTime.Now.Ticks;
    var articleTitle = $"E2E Test Article {timestamp}";
    var articleDescription = "Test article for E2E testing";
    var articleBody = "This is a test article body.";

    await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
    await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
    await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
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
