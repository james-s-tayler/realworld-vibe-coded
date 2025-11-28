using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class GlobalFeedPaginationE2eTests : ConduitPageTest
{
  private const int TotalArticles = 50;
  private const int PageSize = 20;

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
      var (username, token) = await CreateUserViaApiAsync();
      await CreateArticlesViaApiAsync(token, TotalArticles);

      // Navigate to the home page and ensure we're on the Global Feed
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab to ensure we're on the correct tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load
      await Page.WaitForTimeoutAsync(2000);

      // Page 1: Should show articles 50-31 (most recent first)
      await AssertArticleTitlesOnPageAsync(50, 31);

      // Verify pagination control is visible
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Click forward to page 2
      var forwardButton = Page.Locator(".cds--pagination__button--forward");
      await forwardButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Page 2: Should show articles 30-11
      await AssertArticleTitlesOnPageAsync(30, 11);

      // Click forward to page 3
      await forwardButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Page 3: Should show articles 10-1 (last 10 articles)
      await AssertArticleTitlesOnPageAsync(10, 1);

      // Click backward to page 2
      var backwardButton = Page.Locator(".cds--pagination__button--backward");
      await backwardButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Verify we're back on page 2 with articles 30-11
      await AssertArticleTitlesOnPageAsync(30, 11);

      // Click backward to page 1
      await backwardButton.ClickAsync();
      await Page.WaitForTimeoutAsync(1000);

      // Verify we're back on page 1 with articles 50-31
      await AssertArticleTitlesOnPageAsync(50, 31);
    }
    finally
    {
      await SaveTrace("global_feed_pagination_test");
    }
  }

  private async Task AssertArticleTitlesOnPageAsync(int fromNumber, int toNumber)
  {
    var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;

    // Check first article on the page
    var firstArticleTitle = $"Article {fromNumber}";
    var firstArticle = visiblePanel.Locator(".article-preview").Filter(new() { HasText = firstArticleTitle }).First;
    await Expect(firstArticle).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

    // Check last article on the page
    var lastArticleTitle = $"Article {toNumber}";
    var lastArticle = visiblePanel.Locator(".article-preview").Filter(new() { HasText = lastArticleTitle }).First;
    await Expect(lastArticle).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

    // Verify the correct number of articles are displayed
    var articlePreviews = visiblePanel.Locator(".article-preview");
    var expectedCount = fromNumber - toNumber + 1;
    await Expect(articlePreviews).ToHaveCountAsync(expectedCount, new() { Timeout = DefaultTimeout });
  }

  private async Task<(string Username, string Token)> CreateUserViaApiAsync()
  {
    var timestamp = DateTime.Now.Ticks;
    var username = $"paginationuser{timestamp}";
    var email = $"paginationuser{timestamp}@test.com";
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

    return (username, userResponse.User.Token);
  }

  private async Task CreateArticlesViaApiAsync(string token, int count)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(BaseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    // Create articles from 1 to count (so Article 1 is oldest, Article {count} is newest)
    for (var i = 1; i <= count; i++)
    {
      var articleRequest = new
      {
        article = new
        {
          title = $"Article {i}",
          description = $"Description for article {i}",
          body = $"Body content for article {i}",
          tagList = new[] { "pagination-test" },
        },
      };

      var response = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, JsonOptions);
      response.EnsureSuccessStatusCode();

      // Small delay to ensure proper ordering by creation time
      await Task.Delay(10);
    }
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
