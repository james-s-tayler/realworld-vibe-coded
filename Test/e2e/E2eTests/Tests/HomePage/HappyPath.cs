using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2eTests.Tests.HomePage;

/// <summary>
/// Happy path tests for the Home page (/) including feeds and pagination.
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  private const int TotalArticles = 50;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  [Fact]
  public async Task CreatedArticle_AppearsInGlobalFeed()
  {
    // Arrange
    await RegisterUserAsync();
    var (_, articleTitle) = await CreateArticleAsync();

    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Act
    await homePage.ClickGlobalFeedTabAsync();

    // Assert
    await homePage.VerifyArticleVisibleAsync(articleTitle);

    var articlePage = await homePage.ClickArticleAsync(articleTitle);
    await articlePage.VerifyArticleTitleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_IsSelectedByDefaultForUnauthenticatedUser()
  {
    // Arrange
    var uniqueId = GenerateUniqueUsername("deftest");
    var (token, _) = await CreateUserViaApiAsync(uniqueId);

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

    // Act
    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Assert
    await homePage.VerifyGlobalFeedIsSelectedAsync();

    await homePage.VerifyArticleVisibleAsync($"Default Tab Test Article - {uniqueId}");
  }

  [Fact]
  public async Task GlobalFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var uniqueId = GenerateUniqueUsername("pagtest");
    await CreateUserAndArticlesViaApiAsync(uniqueId);

    var homePage = GetHomePage();
    await homePage.GoToAsync();

    await homePage.ClickGlobalFeedTabAsync();

    await homePage.VerifyArticleCountAsync(20);

    // Act
    await homePage.VerifyPaginationVisibleAsync();

    await homePage.ClickNextPageAsync();
    await homePage.ClickNextPageAsync();

    await homePage.ClickPreviousPageAsync();
    await homePage.VerifyArticleCountAsync(20);

    await homePage.ClickPreviousPageAsync();

    // Assert
    await homePage.VerifyArticleCountAsync(20);

    await homePage.VerifyBackwardButtonDisabledAsync();
  }

  [Fact]
  public async Task YourFeed_ShowsArticlesFromFollowedUsers()
  {
    // Arrange
    var testUsername1 = GenerateUniqueUsername("profileuser1");
    var testEmail1 = GenerateUniqueEmail(testUsername1);
    var testPassword1 = "TestPassword123!";
    var testUsername2 = GenerateUniqueUsername("profileuser2");
    var testEmail2 = GenerateUniqueEmail(testUsername2);
    var testPassword2 = "TestPassword123!";

    await RegisterUserAsync(testUsername1, testEmail1, testPassword1);
    var (_, articleTitle) = await CreateArticleAsync();

    await SignOutAsync();

    await RegisterUserAsync(testUsername2, testEmail2, testPassword2);

    var profilePage = GetProfilePage();
    await profilePage.GoToAsync(testUsername1);
    await profilePage.ClickFollowButtonAsync(testUsername1);

    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Act
    await homePage.ClickYourFeedTabAsync();

    // Assert
    await homePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task UserCanFilterArticlesByTag()
  {
    // Arrange
    var testUsername1 = GenerateUniqueUsername("profileuser1");
    var testEmail1 = GenerateUniqueEmail(testUsername1);
    var testPassword1 = "TestPassword123!";

    await RegisterUserAsync(testUsername1, testEmail1, testPassword1);
    var testTag = $"testtag{Guid.NewGuid().ToString("N")[..8]}";
    var (_, articleTitle) = await CreateArticleWithTagAsync(testTag);

    var homePage = GetHomePage();
    await homePage.GoToAsync();

    await homePage.ClickGlobalFeedTabAsync();

    // Act
    await homePage.ClickSidebarTagAsync(testTag);

    // Assert
    await homePage.VerifyTagFilterTabVisibleAsync(testTag);

    await homePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task GlobalFeed_ShowsPaginationWithFewArticles()
  {
    // Arrange
    var uniqueId = GenerateUniqueUsername("fewart");
    var (token, _) = await CreateUserViaApiAsync(uniqueId);
    await CreateArticlesForUserAsync(token, 5, uniqueId);

    var homePage = GetHomePage();
    await homePage.GoToAsync();

    // Act
    await homePage.ClickGlobalFeedTabAsync();

    // Assert
    await homePage.VerifyArticlesLoadedAsync();

    await homePage.VerifyPaginationVisibleAsync();
  }

  [Fact]
  public async Task YourFeed_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var uniqueId1 = GenerateUniqueUsername("feeduser1");
    var uniqueId2 = GenerateUniqueUsername("feeduser2");
    var (user1Token, user1Username) = await CreateUserViaApiAsync(uniqueId1);
    var (user2Token, _) = await CreateUserViaApiAsync(uniqueId2);

    await CreateArticlesForUserAsync(user1Token, TotalArticles, uniqueId1);

    await FollowUserAsync(user2Token, user1Username);

    var loginPage = GetLoginPage();
    await loginPage.GoToAsync();
    await loginPage.LoginAsync($"{uniqueId2}@test.com", "TestPassword123!");

    var homePage = GetHomePage();

    // Act
    await homePage.ClickYourFeedTabAsync();

    // Assert
    await homePage.VerifyArticleCountAsync(20);

    await homePage.VerifyPaginationVisibleAsync();

    await homePage.ClickNextPageAsync();

    await homePage.ClickNextPageAsync();
    await homePage.VerifyArticleCountAsync(10);

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
