using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldArticleInteractionsE2eTests : ConduitPageTest
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
      var editButton = Page.Locator("button").Filter(new() { HasText = "Edit Article" });
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

      // Click delete button on article page - this shows a confirm dialog
      var deleteButton = Page.GetByRole(AriaRole.Button, new() { Name = "Delete Article" });
      await deleteButton.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Handle the confirm dialog - automatically accept it
      Page.Dialog += async (_, dialog) =>
      {
        await dialog.AcceptAsync();
      };

      await deleteButton.ClickAsync();

      // Verify redirect to home page after deletion
      await Expect(Page).ToHaveURLAsync(BaseUrl, new() { Timeout = DefaultTimeout });

      // Check that the deleted article is not in the feed (go to Global Feed)
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Check that the deleted article is not in the feed
      var deletedArticle = Page.GetByText(articleTitle).First;
      await Expect(deletedArticle).Not.ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
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
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for article preview to be visible before clicking
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreview = visiblePanel.Locator(".article-preview").Filter(new() { HasText = articleTitle }).First;
      await Expect(articlePreview).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click on the article link
      var articleLink = articlePreview.Locator(".article-link");
      await articleLink.ClickAsync();
      await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Find favorite button - text contains "Favorite Article"
      var favoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Favorite Article") });
      await favoriteButton.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Click to favorite
      await favoriteButton.ClickAsync();

      // Verify button changed to unfavorite (implicitly waits)
      var unfavoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Unfavorite Article") });
      await Expect(unfavoriteButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click to unfavorite
      await unfavoriteButton.ClickAsync();

      // Verify button changed back to favorite (implicitly waits)
      favoriteButton = Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Favorite Article") });
      await Expect(favoriteButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
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
      var commentTextarea = Page.GetByPlaceholder("Write a comment...");
      await commentTextarea.WaitForAsync(new() { Timeout = DefaultTimeout });
      await commentTextarea.FillAsync(commentText);

      var postButton = Page.Locator("button").Filter(new() { HasText = "Post Comment" });
      await postButton.ClickAsync();

      // Verify comment is displayed (implicitly waits for it to appear)
      var comment = Page.GetByText(commentText).First;
      await Expect(comment).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
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
      var commentTextarea = Page.GetByPlaceholder("Write a comment...");
      await commentTextarea.WaitForAsync(new() { Timeout = DefaultTimeout });
      await commentTextarea.FillAsync(commentText);

      var postButton = Page.GetByRole(AriaRole.Button, new() { Name = "Post Comment" });
      await postButton.ClickAsync();

      // Wait for comment to appear
      var comment = Page.GetByText(commentText);
      await comment.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Set up response wait before clicking delete
      var responseTask = Page.WaitForResponseAsync(
        response => response.Url.Contains("/api/articles/") && response.Url.Contains("/comments/") && response.Request.Method == "DELETE",
        new() { Timeout = DefaultTimeout });

      // Find and click delete button on the comment - it's a button with trash icon
      var deleteButton = Page.Locator(".mod-options").First;
      await deleteButton.ClickAsync();

      // Wait for the delete API response
      await responseTask;

      // Verify comment is no longer visible (implicitly waits for the UI to update)
      await Expect(comment).Not.ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("delete_comment_test");
    }
  }

  private async Task<string> CreateArticle()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
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

  private async Task<string> CreateArticle(string username)
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

    var articleTitle = $"{username} Article {GenerateUniqueUsername("art")}";
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
