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
      var timestamp = DateTime.Now.Ticks;
      var token = await CreateUserAndArticlesViaApiAsync(timestamp);

      // Navigate to the home page
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Click on Global Feed tab to ensure we're on the correct tab
      var globalFeedTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });
      await globalFeedTab.WaitForAsync(new() { Timeout = DefaultTimeout });
      await globalFeedTab.ClickAsync();

      // Wait for articles to load by checking for article previews
      var visiblePanel = Page.GetByRole(AriaRole.Tabpanel).First;
      var articlePreviews = visiblePanel.Locator(".article-preview");

      // Wait for at least 20 articles to be loaded (first page should show 20)
      await Expect(articlePreviews).ToHaveCountAsync(20, new() { Timeout = DefaultTimeout });

      // Verify pagination control is visible (should appear when there are > 20 articles)
      var pagination = Page.Locator(".cds--pagination");
      await Expect(pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Get the initial page number display
      var pageStatus = Page.Locator(".cds--pagination__text").First;
      var initialPageText = await pageStatus.TextContentAsync();

      // Click forward button to go to page 2
      var forwardButton = Page.Locator(".cds--pagination__button--forward");
      await Expect(forwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await forwardButton.ClickAsync();

      // Wait for page content to change by waiting for articles to reload
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify we moved to a different page (page status text should change)
      await Expect(pageStatus).Not.ToHaveTextAsync(initialPageText ?? string.Empty, new() { Timeout = DefaultTimeout });

      // Click forward again to page 3
      await forwardButton.ClickAsync();
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Now navigate backward
      var backwardButton = Page.Locator(".cds--pagination__button--backward");
      await Expect(backwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
      await backwardButton.ClickAsync();

      // Wait for page content to change
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Navigate back to first page
      await backwardButton.ClickAsync();
      await Expect(articlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify backward button is disabled on first page
      await Expect(backwardButton).ToBeDisabledAsync(new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("global_feed_pagination_test");
    }
  }

  private async Task<string> CreateUserAndArticlesViaApiAsync(long timestamp)
  {
    var username = $"paginationuser{timestamp}";
    var email = $"paginationuser{timestamp}@test.com";
    var password = "TestPassword123!";

    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(BaseUrl);

    // Create user
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
    var token = userResponse.User.Token;

    // Create articles
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    for (var i = 1; i <= TotalArticles; i++)
    {
      var articleRequest = new
      {
        article = new
        {
          title = $"Pagination Test Article {i} - {timestamp}",
          description = $"Description for article {i}",
          body = $"Body content for article {i}",
          tagList = new[] { "pagination-test" },
        },
      };

      var articleResponse = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, JsonOptions);
      articleResponse.EnsureSuccessStatusCode();

      // Small delay to ensure proper ordering by creation time
      await Task.Delay(10);
    }

    return token;
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
