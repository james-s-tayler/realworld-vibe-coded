using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2eTests;

/// <summary>
/// Provides API-based data setup methods for E2E tests.
/// This fixture enables tests to arrange data via API calls instead of UI interactions.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
  private readonly string _baseUrl;
  private readonly JsonSerializerOptions _jsonOptions;

  public ApiFixture()
  {
    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";
    _jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
  }

  public ValueTask InitializeAsync()
  {
    return ValueTask.CompletedTask;
  }

  public ValueTask DisposeAsync()
  {
    return ValueTask.CompletedTask;
  }

  /// <summary>
  /// Creates a user via API and returns the authentication token and username.
  /// </summary>
  public async Task<(string Token, string Username)> CreateUserAsync(string username, string email, string password)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);

    var registerRequest = new
    {
      user = new
      {
        username,
        email,
        password,
      },
    };

    var response = await httpClient.PostAsJsonAsync("/api/users", registerRequest, _jsonOptions);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, _jsonOptions)!;
    return (userResponse.User.Token, username);
  }

  /// <summary>
  /// Creates an article for the authenticated user and returns the article slug and title.
  /// </summary>
  public async Task<(string Slug, string Title)> CreateArticleAsync(
    string token,
    string title,
    string description,
    string body,
    string[]? tags = null)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    var articleRequest = new
    {
      article = new
      {
        title,
        description,
        body,
        tagList = tags ?? Array.Empty<string>(),
      },
    };

    var response = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, _jsonOptions);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var articleResponse = JsonSerializer.Deserialize<ArticleResponse>(responseContent, _jsonOptions)!;
    return (articleResponse.Article.Slug, articleResponse.Article.Title);
  }

  /// <summary>
  /// Creates multiple articles for the authenticated user.
  /// </summary>
  public async Task CreateArticlesAsync(string token, int count, string uniqueIdPrefix)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    for (var i = 1; i <= count; i++)
    {
      var articleRequest = new
      {
        article = new
        {
          title = $"Test Article {i} - {uniqueIdPrefix}",
          description = $"Description for article {i}",
          body = $"Body content for article {i}",
          tagList = new[] { "test" },
        },
      };

      var response = await httpClient.PostAsJsonAsync("/api/articles", articleRequest, _jsonOptions);
      response.EnsureSuccessStatusCode();
    }
  }

  /// <summary>
  /// Creates a comment on an article.
  /// </summary>
  public async Task CreateCommentAsync(string token, string articleSlug, string commentBody)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    var commentRequest = new
    {
      comment = new
      {
        body = commentBody,
      },
    };

    var response = await httpClient.PostAsJsonAsync($"/api/articles/{articleSlug}/comments", commentRequest, _jsonOptions);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Follows a user.
  /// </summary>
  public async Task FollowUserAsync(string followerToken, string usernameToFollow)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {followerToken}");

    var response = await httpClient.PostAsync($"/api/profiles/{usernameToFollow}/follow", null);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Favorites an article.
  /// </summary>
  public async Task FavoriteArticleAsync(string token, string articleSlug)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

    var response = await httpClient.PostAsync($"/api/articles/{articleSlug}/favorite", null);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Logs in a user and returns the authentication token.
  /// </summary>
  public async Task<string> LoginAsync(string email, string password)
  {
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(_baseUrl);

    var loginRequest = new
    {
      user = new
      {
        email,
        password,
      },
    };

    var response = await httpClient.PostAsJsonAsync("/api/users/login", loginRequest, _jsonOptions);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, _jsonOptions)!;
    return userResponse.User.Token;
  }

  // DTOs for API responses
  private class UserResponse
  {
    [JsonPropertyName("user")]
    public UserData User { get; set; } = null!;
  }

  private class UserData
  {
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
  }

  private class ArticleResponse
  {
    [JsonPropertyName("article")]
    public ArticleData Article { get; set; } = null!;
  }

  private class ArticleData
  {
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
  }
}
