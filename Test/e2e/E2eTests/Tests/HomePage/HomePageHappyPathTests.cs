using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2eTests.Tests.HomePage;

/// <summary>
/// Happy path tests for the Home page (/) including feeds and pagination.
/// </summary>
[Collection("E2E Tests")]
public class HomePageHappyPathTests : AppPageTest
{
  private const int TotalArticles = 50;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  [Fact]
  public async Task CreatedArticle_AppearsInGlobalFeed()
  {
    // First, sign up and create an article
    await RegisterUserAsync();
    var (_, articleTitle) = await CreateArticleAsync();

    // Navigate to home page using page model
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Click on Global Feed tab
    await homePage.ClickGlobalFeedTabAsync();

    // Verify article is visible
    await homePage.VerifyArticleVisibleAsync(articleTitle);

    // Click on article link to view it
    var articlePage = await homePage.ClickArticleAsync(articleTitle);
    await articlePage.VerifyArticleTitleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_IsSelectedByDefaultForUnauthenticatedUser()
  {
    // Setup: Create user and one article via API so global feed has content
    var uniqueId = GenerateUniqueUsername("deftest");
    var (token, _) = await CreateUserViaApiAsync(uniqueId);

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

    // Navigate to home page as unauthenticated user (no login) using page model
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Verify Global Feed tab is selected by default
    await homePage.VerifyGlobalFeedIsSelectedAsync();

    // Verify article preview is visible
    await homePage.VerifyArticleVisibleAsync($"Default Tab Test Article - {uniqueId}");
  }

  [Fact]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Setup: Create user and 50 articles via API
    var uniqueId = GenerateUniqueUsername("pagtest");
    await CreateUserAndArticlesViaApiAsync(uniqueId);

    // Navigate to home page using page model
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Click on Global Feed tab
    await homePage.ClickGlobalFeedTabAsync();

    // Wait for articles to be loaded (first page should show 20 by default)
    await homePage.VerifyArticleCountAsync(20);

    // Verify pagination control is visible
    await homePage.VerifyPaginationVisibleAsync();

    // Navigate through pages
    await homePage.ClickNextPageAsync();
    await homePage.ClickNextPageAsync();

    // Navigate backward
    await homePage.ClickPreviousPageAsync();
    await homePage.VerifyArticleCountAsync(20);

    // Navigate back to first page
    await homePage.ClickPreviousPageAsync();
    await homePage.VerifyArticleCountAsync(20);

    // Verify backward button is disabled on first page
    await homePage.VerifyBackwardButtonDisabledAsync();
  }

  [Fact]
  public async Task YourFeed_ShowsArticlesFromFollowedUsers()
  {
    var testUsername1 = GenerateUniqueUsername("profileuser1");
    var testEmail1 = GenerateUniqueEmail(testUsername1);
    var testPassword1 = "TestPassword123!";
    var testUsername2 = GenerateUniqueUsername("profileuser2");
    var testEmail2 = GenerateUniqueEmail(testUsername2);
    var testPassword2 = "TestPassword123!";

    // Register first user and create an article
    await RegisterUserAsync(testUsername1, testEmail1, testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    // Sign out
    await SignOutAsync();

    // Register second user
    await RegisterUserAsync(testUsername2, testEmail2, testPassword2);

    // Follow first user using profile page model
    var profilePage = GetProfilePage();
    await profilePage.GoToAsync(testUsername1);
    await profilePage.ClickFollowButtonAsync(testUsername1);

    // Navigate to home page
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Click on Your Feed tab
    await homePage.ClickYourFeedTabAsync();

    // Verify followed user's article appears in Your Feed
    await homePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFilterArticlesByTag()
  {
    var testUsername1 = GenerateUniqueUsername("profileuser1");
    var testEmail1 = GenerateUniqueEmail(testUsername1);
    var testPassword1 = "TestPassword123!";

    // Register user and create an article with a specific tag
    await RegisterUserAsync(testUsername1, testEmail1, testPassword1);
    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var (_, articleTitle) = await CreateArticleWithTagAsync(testTag);

    // Navigate to home page
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // First go to Global Feed to see the article
    await homePage.ClickGlobalFeedTabAsync();

    // Click on tag in sidebar
    await homePage.ClickSidebarTagAsync(testTag);

    // Verify the tag filter tab is visible
    await homePage.VerifyTagFilterTabVisibleAsync(testTag);

    // Verify the article with that tag is displayed
    await homePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    // Setup: Create user and only 5 articles (less than page size of 20)
    var uniqueId = GenerateUniqueUsername("fewart");
    var (token, _) = await CreateUserViaApiAsync(uniqueId);
    await CreateArticlesForUserAsync(token, 5, uniqueId);

    // Navigate to the home page using page model
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Click on Global Feed tab
    await homePage.ClickGlobalFeedTabAsync();

    // Wait for at least one article to be loaded
    await homePage.VerifyArticlesLoadedAsync();

    // Verify pagination control is visible
    await homePage.VerifyPaginationVisibleAsync();
  }

  [Fact]
  public async Task YourFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Setup: Create two users and have user2 follow user1
    var uniqueId1 = GenerateUniqueUsername("feeduser1");
    var uniqueId2 = GenerateUniqueUsername("feeduser2");
    var (user1Token, user1Username) = await CreateUserViaApiAsync(uniqueId1);
    var (user2Token, _) = await CreateUserViaApiAsync(uniqueId2);

    // User1 creates 50 articles
    await CreateArticlesForUserAsync(user1Token, TotalArticles, uniqueId1);

    // User2 follows user1
    await FollowUserAsync(user2Token, user1Username);

    // Navigate to login page and login as user2
    var loginPage = GetLoginPage();
    await loginPage.GoToAsync();
    await loginPage.LoginAsync($"{uniqueId2}@test.com", "TestPassword123!");

    // Navigate to home page using page model
    var homePage = GetHomePage();

    // Click on Your Feed tab
    await homePage.ClickYourFeedTabAsync();

    // Wait for articles to be loaded (first page should show 20 by default)
    await homePage.VerifyArticleCountAsync(20);

    // Verify pagination control is visible
    await homePage.VerifyPaginationVisibleAsync();

    // Click forward button to go to page 2
    await homePage.ClickNextPageAsync();

    // Click forward again to page 3 (should show remaining 10 articles)
    await homePage.ClickNextPageAsync();
    await homePage.VerifyArticleCountAsync(10);

    // Navigate backward
    await homePage.ClickPreviousPageAsync();
    await homePage.VerifyArticleCountAsync(20);
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
