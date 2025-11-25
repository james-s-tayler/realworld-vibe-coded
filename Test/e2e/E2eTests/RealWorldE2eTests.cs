using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldE2eTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanSignUp_ViewProfile_AndEditProfile()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign Up, View and Edit Profile Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Navigate to home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Sign up link
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/register", new() { Timeout = DefaultTimeout });

      // Fill in registration form
      await Page.GetByPlaceholder("Username").FillAsync(TestUsername);
      await Page.GetByPlaceholder("Email").FillAsync(TestEmail);
      await Page.GetByPlaceholder("Password").FillAsync(TestPassword);

      // Submit registration
      var responseTask = Page.WaitForResponseAsync(
        response =>
        response.Url.Contains("/api/users") && response.Request.Method == "POST",
        new() { Timeout = DefaultTimeout });

      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

      await responseTask;

      // Wait for the user link to appear in the header (indicates successful registration and navigation)
      var userLink = Page.GetByRole(AriaRole.Link, new() { Name = TestUsername }).First;
      await userLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
      Assert.True(await userLink.IsVisibleAsync(), "User link should be visible in header after sign up");

      // Click on user profile link
      await userLink.ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/profile/{TestUsername}", new() { Timeout = DefaultTimeout });

      // Verify profile page elements
      var profileUsername = Page.GetByRole(AriaRole.Heading, new() { Name = TestUsername });
      await profileUsername.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await profileUsername.IsVisibleAsync(), "Username should be displayed on profile page");

      // Navigate to settings to edit profile
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/settings", new() { Timeout = DefaultTimeout });

      // Update bio
      var bioInput = Page.GetByPlaceholder("Short bio about you");
      await bioInput.WaitForAsync(new() { Timeout = DefaultTimeout });
      await bioInput.FillAsync("This is my updated bio for E2E test");

      // Submit settings update
      await Page.GetByRole(AriaRole.Button, new() { Name = "Update Settings" }).ClickAsync();

      // Wait for success message
      var successMessage = Page.GetByText("Settings updated successfully");
      await Expect(successMessage).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Go back to profile to verify bio was updated
      await Page.GetByRole(AriaRole.Link, new() { Name = TestUsername }).First.ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/profile/{TestUsername}", new() { Timeout = DefaultTimeout });

      // Wait for profile to load and verify bio is displayed
      await Page.WaitForTimeoutAsync(1000);
      var bioText = Page.GetByText("This is my updated bio for E2E test");
      await Expect(bioText).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("user_profile_test");
    }
  }

  [Fact]
  public async Task UserCanCreateArticle_AndViewArticle()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Create and View Article Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, sign up a user
      await RegisterUser();

      // Navigate to new article page
      await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

      // Fill in article form
      var timestamp = DateTime.Now.Ticks;
      var articleTitle = $"E2E Test Article {timestamp}";
      var articleDescription = "This is a test article created by E2E tests";
      var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
      var articleTag = "e2etest";

      await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
      await Page.GetByPlaceholder("What's this article about?").FillAsync(articleDescription);
      await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync(articleBody);
      await Page.GetByPlaceholder("Enter tags").FillAsync(articleTag);

      // Submit article
      await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

      // Wait for redirect to article page
      await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Verify article content
      var articleHeading = Page.GetByRole(AriaRole.Heading, new() { Name = articleTitle });
      await articleHeading.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await articleHeading.IsVisibleAsync(), "Article title should be displayed");

      // Verify author
      var authorLink = Page.GetByRole(AriaRole.Link, new() { Name = TestUsername }).First;
      Assert.True(await authorLink.IsVisibleAsync(), "Author name should be displayed");
    }
    finally
    {
      await SaveTrace("create_article_test");
    }
  }

  [Fact]
  public async Task CreatedArticle_AppearsInGlobalFeed()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Article Appears in Global Feed Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, sign up and create an article
      await RegisterUser();
      var articleTitle = await CreateArticle();

      // Navigate to home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify article appears in feed - look for the article title link
      var articleLink = Page.GetByRole(AriaRole.Link, new() { Name = articleTitle });
      await Expect(articleLink).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click on article to view it
      await articleLink.ClickAsync();

      // Verify we're on the article page
      await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
      var articleHeading = Page.GetByRole(AriaRole.Heading, new() { Name = articleTitle });
      Assert.True(await articleHeading.IsVisibleAsync(), "Article should be viewable from Global Feed");
    }
    finally
    {
      await SaveTrace("global_feed_test");
    }
  }

  private async Task<string> CreateArticle()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

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
}
