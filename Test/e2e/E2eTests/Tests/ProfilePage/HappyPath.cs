using System.Net.Http.Json;
using static E2eTests.PageModels.Pages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Happy path tests for the Profile page (/profile/:username).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  private const int TotalArticles = 50;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;
  private string _testUsername2 = null!;
  private string _testEmail2 = null!;
  private string _testPassword2 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("profileuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";

    _testUsername2 = GenerateUniqueUsername("profileuser2");
    _testEmail2 = GenerateUniqueEmail(_testUsername2);
    _testPassword2 = "TestPassword123!";
  }

  [Fact]
  public async Task UserCanViewOtherUsersProfile()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    await CreateArticleAsync();

    await SignOutAsync();

    await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

    // Act
    await Pages.ProfilePage.GoToAsync(_testUsername1);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(_testUsername1);
    await Pages.ProfilePage.VerifyMyArticlesTabVisibleAsync();
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    await CreateArticleAsync();

    await SignOutAsync();

    await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

    await Pages.ProfilePage.GoToAsync(_testUsername1);

    // Act + Assert
    await Pages.ProfilePage.ClickFollowButtonAsync(_testUsername1);
    await Pages.ProfilePage.ClickUnfollowButtonAsync(_testUsername1);
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    var articleTitle = await CreateArticleAsync();

    await SignOutAsync();

    await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    await Pages.HomePage.ClickArticleAsync(articleTitle);
    await Pages.ArticlePage.ClickFavoriteButtonAsync();

    await Pages.ProfilePage.GoToAsync(_testUsername2);

    // Act
    await Pages.ProfilePage.ClickFavoritedArticlesTabAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleVisibleAsync(articleTitle);
  }

  [Fact]
  public async Task ProfilePage_MyArticles_DisplaysPaginationAndNavigatesCorrectly()
  {
    // Arrange
    var uniqueId = GenerateUniqueUsername("profileuser");
    var (token, username) = await CreateUserViaApiAsync(uniqueId);
    await CreateArticlesForUserAsync(token, TotalArticles, uniqueId);

    await Pages.ProfilePage.GoToAsync(username);

    await Pages.ProfilePage.WaitForArticlesToLoadAsync();

    // Act
    await Pages.ProfilePage.VerifyArticleCountAsync(20);

    await Pages.ProfilePage.VerifyPaginationVisibleAsync();

    await Pages.ProfilePage.ClickNextPageAsync();
    await Pages.ProfilePage.ClickNextPageAsync();

    // Assert
    await Pages.ProfilePage.VerifyArticleCountAsync(10);

    await Pages.ProfilePage.ClickPreviousPageAsync();
    await Pages.ProfilePage.VerifyArticleCountAsync(20);
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
