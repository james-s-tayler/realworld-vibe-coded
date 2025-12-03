using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class GlobalFeedPaginationE2eTests : ConduitPageTest
{
  private const int TotalArticles = 50;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  [Fact]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
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
      // Setup: Create user and 50 articles via API
      var uniqueId = GenerateUniqueUsername("pagtest");
      var token = await CreateUserAndArticlesViaApiAsync(uniqueId);

      // Navigate to the home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab to ensure we're on the correct tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load by checking for article previews
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreviews = visiblePanel.Locator(".article-preview");

      // Wait for articles to be loaded (first page should show 20 by default)
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify pagination control is visible (should appear when there is at least 1 article)
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward button to go to page 2
      var forwardButton = Page.Locator(".cds--pagination__button--forward");
      await Expect(forwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await forwardButton.ClickAsync();

      // Wait for page content to change by waiting for articles to reload
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward again to page 3 (should show remaining articles - count may vary if other tests created articles)
      await forwardButton.ClickAsync();
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Now navigate backward
      var backwardButton = Page.Locator(".cds--pagination__button--backward");
      await Expect(backwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await backwardButton.ClickAsync();

      // Wait for page content to change - should be back to 20 articles
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Navigate back to first page
      await backwardButton.ClickAsync();
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify backward button is disabled on first page
      await Expect(backwardButton).ToBeDisabledAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("global_feed_pagination_test");
    }
  }

  [Fact]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Global Feed Pagination With Few Articles Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Setup: Create user and only 5 articles (less than page size of 20)
      var uniqueId = GenerateUniqueUsername("fewart");
      var (token, _) = await CreateUserViaApiAsync(uniqueId);
      await CreateArticlesForUserAsync(token, 5, uniqueId);

      // Navigate to the home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab to ensure we're on the correct tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load by checking for article previews
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreviews = visiblePanel.Locator(".article-preview");

      // Wait for at least one article to be loaded (count may vary due to other tests)
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify pagination control is visible even with few articles (should appear when there is at least 1 article)
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("global_feed_pagination_few_articles_test");
    }
  }

  [Fact]
  public async Task GlobalFeed_IsSelectedByDefaultForUnauthenticatedUser()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Global Feed Default Selection Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Setup: Create user and one article via API so global feed has content
      var uniqueId = GenerateUniqueUsername("deftest");
      var (token, username) = await CreateUserViaApiAsync(uniqueId);

      // Create a single article
      using var httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri(BaseUrl);
      httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

      var articleRequest = new
      {
        article = new
        {
          title = $"Default Tab Test Article - {uniqueId}",
          description = "Test article for default tab selection",
          body = "This article verifies the global feed is selected by default",
          tagList = new[] { "default-test" },
        },
      };

      var articleResponse = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, JsonOptions, TestContext.Current.CancellationToken);
      articleResponse.EnsureSuccessStatusCode();

      // Navigate to home page as unauthenticated user (no login)
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Wait for the page to fully load - the Global Feed tab should be selected by default
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await Expect(globalFeedTab).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify Global Feed tab is selected (has aria-selected="true")
      await Expect(globalFeedTab).ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = DefaultTimeout });

      // Verify article preview is visible on the home page without clicking any tab
      var articlePreview = Page.Locator(".article-preview").First;
      await Expect(articlePreview).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify our test article title is visible somewhere on the page
      var articleTitle = Page.GetByText($"Default Tab Test Article - {uniqueId}");
      await Expect(articleTitle).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("global_feed_default_selection_test");
    }
  }

  [Fact]
  public async Task YourFeed_DisplaysPaginationAndNavigatesCorrectly()
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
      // Setup: Create two users and have user2 follow user1
      var uniqueId1 = GenerateUniqueUsername("feeduser1");
      var uniqueId2 = GenerateUniqueUsername("feeduser2");
      var (user1Token, user1Username) = await CreateUserViaApiAsync(uniqueId1);
      var (user2Token, user2Username) = await CreateUserViaApiAsync(uniqueId2);

      // User1 creates 50 articles
      await CreateArticlesForUserAsync(user1Token, TotalArticles, uniqueId1);

      // User2 follows user1
      await FollowUserAsync(user2Token, user1Username);

      // Navigate to home page and login as user2
      await Page.GotoAsync($"{BaseUrl}/login", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Login as user2
      await Page.GetByLabel("Email").FillAsync($"{uniqueId2}@test.com");
      await Page.GetByLabel("Password").FillAsync("TestPassword123!");
      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

      // Wait for redirect to home page
      await Expect(Page).ToHaveURLAsync(BaseUrl + "/", new() { Timeout = DefaultTimeout });

      // Your Feed should be the first tab for authenticated users
      var yourFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Your Feed" });
      await yourFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await yourFeedTab.ClickAsync();

      // Wait for articles to load - scope to the active tabpanel
      var yourFeedPanel = Page.Locator("[role='tabpanel']").First;
      var articlePreviews = yourFeedPanel.Locator(".article-preview");

      // Wait for articles to be loaded (first page should show 20 by default)
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify pagination control is visible within the Your Feed panel
      var pagination = yourFeedPanel.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward button to go to page 2
      var forwardButton = yourFeedPanel.Locator(".cds--pagination__button--forward");
      await Expect(forwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await forwardButton.ClickAsync();

      // Wait for page content to change
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward again to page 3 (should show remaining 10 articles)
      await forwardButton.ClickAsync();
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      await Expect(articlePreviews).ToHaveCountAsync(10, new() { Timeout = DefaultTimeout });

      // Navigate backward
      var backwardButton = yourFeedPanel.Locator(".cds--pagination__button--backward");
      await Expect(backwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await backwardButton.ClickAsync();

      // Wait for page content to change - should be back to 20 articles
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify backward button is enabled on page 2
      await Expect(backwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("your_feed_pagination_test");
    }
  }

  [Fact]
  public async Task ProfilePage_MyArticles_DisplaysPaginationAndNavigatesCorrectly()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Profile Page My Articles Pagination Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Setup: Create user and 50 articles
      var uniqueId = GenerateUniqueUsername("profileuser");
      var (token, username) = await CreateUserViaApiAsync(uniqueId);
      await CreateArticlesForUserAsync(token, TotalArticles, uniqueId);

      // Navigate to profile page
      await Page.GotoAsync($"{BaseUrl}/profile/{username}", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Wait for My Articles tab to be visible
      var myArticlesTab = Page.GetByRole(AriaRole.Tab, new() { Name = "My Articles" });
      await myArticlesTab.WaitForAsync(new() { Timeout = DefaultTimeout });

      // Scope to the active tabpanel for My Articles
      var myArticlesPanel = Page.Locator("[role='tabpanel']").First;
      var articlePreviews = myArticlesPanel.Locator(".article-preview");

      // Wait for articles to load
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify pagination control is visible
      var pagination = myArticlesPanel.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward button to go to page 2
      var forwardButton = myArticlesPanel.Locator(".cds--pagination__button--forward");
      await Expect(forwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await forwardButton.ClickAsync();

      // Wait for page content to change
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward again to page 3 (should show remaining 10 articles)
      await forwardButton.ClickAsync();
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      await Expect(articlePreviews).ToHaveCountAsync(10, new() { Timeout = DefaultTimeout });

      // Navigate backward
      var backwardButton = myArticlesPanel.Locator(".cds--pagination__button--backward");
      await Expect(backwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await backwardButton.ClickAsync();

      // Wait for page content to change - should be back to 20 articles
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("profile_my_articles_pagination_test");
    }
  }

  private async Task<string> CreateUserAndArticlesViaApiAsync(string uniqueId)
  {
    var (token, _) = await CreateUserViaApiAsync(uniqueId);
    await CreateArticlesForUserAsync(token, TotalArticles, uniqueId);
    return token;
  }

  private async Task<(string Token, string Username)> CreateUserViaApiAsync(string uniqueId)
  {
    var username = uniqueId;
    var email = $"{uniqueId}@test.com";
    var password = "TestPassword123!";

    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(BaseUrl);

    var registerRequest = new
    {
      user = new
      {
        username,
        email,
        password,
      },
    };

    var response = await httpClient.PostAsJsonAsync("/api/users", registerRequest, JsonOptions);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, JsonOptions)!;
    return (userResponse.User.Token, username);
  }

  private async Task CreateArticlesForUserAsync(string token, int count, string uniqueId)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(BaseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    for (var i = 1; i <= count; i++)
    {
      var articleRequest = new
      {
        article = new
        {
          title = $"Pagination Test Article {i} - {uniqueId}",
          description = $"Description for article {i}",
          body = $"Body content for article {i}",
          tagList = new[] { "pagination-test" },
        },
      };

      var articleResponse = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, JsonOptions);
      articleResponse.EnsureSuccessStatusCode();
    }
  }

  private async Task FollowUserAsync(string followerToken, string usernameToFollow)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(BaseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {followerToken}");

    var response = await httpClient.PostAsync($"/api/profiles/{usernameToFollow}/follow", null);
    response.EnsureSuccessStatusCode();
  }

  private class UserResponse
  {
    [JsonPropertyName("user")]
    public UserData User { get; set; } = null!;
  }

  private class UserData
  {
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
  }
}
