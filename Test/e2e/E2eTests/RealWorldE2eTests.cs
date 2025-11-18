using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldE2eTests : PageTest
{
  private const int DefaultTimeout = 10000;
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

    // Use environment variable for URL if available (for Docker), otherwise use localhost
    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    // Generate unique credentials for this test run
    var timestamp = DateTime.Now.Ticks;
    _testUsername = $"testuser{timestamp}";
    _testEmail = $"testuser{timestamp}@test.com";
    _testPassword = "TestPassword123!";

    // Configure context
    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

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
      await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Sign up link
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/register", new() { Timeout = DefaultTimeout });

      // Fill in registration form
      await Page.GetByPlaceholder("Username").FillAsync(_testUsername);
      await Page.GetByPlaceholder("Email").FillAsync(_testEmail);
      await Page.GetByPlaceholder("Password").FillAsync(_testPassword);

      // Submit registration
      var responseTask = Page.WaitForResponseAsync(
        response =>
        response.Url.Contains("/api/users") && response.Request.Method == "POST",
        new() { Timeout = DefaultTimeout });

      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

      await responseTask;

      // Wait for the user link to appear in the header (indicates successful registration and navigation)
      var userLink = Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First;
      await userLink.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
      Assert.True(await userLink.IsVisibleAsync(), "User link should be visible in header after sign up");

      // Click on user profile link
      await userLink.ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/profile/{_testUsername}", new() { Timeout = DefaultTimeout });

      // Verify profile page elements
      var profileUsername = Page.GetByRole(AriaRole.Heading, new() { Name = _testUsername });
      await profileUsername.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await profileUsername.IsVisibleAsync(), "Username should be displayed on profile page");

      // Navigate to settings to edit profile
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/settings", new() { Timeout = DefaultTimeout });

      // Update bio
      var bioInput = Page.GetByPlaceholder("Short bio about you");
      await bioInput.WaitForAsync(new() { Timeout = DefaultTimeout });
      await bioInput.FillAsync("This is my updated bio for E2E test");

      // Submit settings update
      await Page.GetByRole(AriaRole.Button, new() { Name = "Update Settings" }).ClickAsync();

      // Wait for success message
      var successMessage = Page.Locator("text=/settings updated successfully/i");
      await successMessage.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await successMessage.IsVisibleAsync(), "Success message should appear after updating profile");

      // Go back to profile to verify bio was updated
      await Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First.ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/profile/{_testUsername}", new() { Timeout = DefaultTimeout });

      // Verify bio is displayed on profile
      var bioText = Page.Locator("text=/This is my updated bio/i");
      await bioText.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await bioText.IsVisibleAsync(), "Updated bio should be visible on profile page");
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
      await SignUpUser();

      // Navigate to new article page
      await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
      await Page.WaitForURLAsync($"{_baseUrl}/editor", new() { Timeout = DefaultTimeout });

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

      // Verify article body
      var bodyContent = Page.Locator("text=/Test Article/i");
      await bodyContent.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await bodyContent.IsVisibleAsync(), "Article body should be displayed");

      // Verify author
      var authorLink = Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First;
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
      await SignUpUser();
      var articleTitle = await CreateArticle();

      // Navigate to home page
      await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Verify article appears in feed
      var articleInFeed = Page.Locator($"text=/{articleTitle}/i").First;
      await articleInFeed.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await articleInFeed.IsVisibleAsync(), "Created article should appear in Global Feed");

      // Click on article to view it
      await articleInFeed.ClickAsync();

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

  // Helper methods
  private async Task SignUpUser()
  {
    await Page.GotoAsync(_baseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
    await Page.WaitForURLAsync($"{_baseUrl}/register", new() { Timeout = DefaultTimeout });

    // Fill in the form
    await Page.GetByPlaceholder("Username").FillAsync(_testUsername);
    await Page.GetByPlaceholder("Email").FillAsync(_testEmail);
    await Page.GetByPlaceholder("Password").FillAsync(_testPassword);

    // Submit the form directly using JavaScript and wait for navigation
    await Page.RunAndWaitForNavigationAsync(
      async () => await Page.EvaluateAsync("document.querySelector('form').requestSubmit()"),
      new() { UrlString = _baseUrl, Timeout = DefaultTimeout });

    // Wait for the user link to appear in the header to confirm login completed
    await Page.GetByRole(AriaRole.Link, new() { Name = _testUsername }).First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
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
