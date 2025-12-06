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
  private readonly HttpClient _httpClient;
  private int _userCounter;
  private int _articleCounter;

  public ApiFixture()
  {
    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";
    _jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    _httpClient = new HttpClient
    {
      BaseAddress = new Uri(_baseUrl),
    };
  }

  public ValueTask InitializeAsync()
  {
    return ValueTask.CompletedTask;
  }

  public ValueTask DisposeAsync()
  {
    _httpClient?.Dispose();
    return ValueTask.CompletedTask;
  }

  /// <summary>
  /// Creates a user via API and returns the authentication token, username, email, and password.
  /// The fixture generates unique test data automatically.
  /// </summary>
  public async Task<(string Token, string Username, string Email, string Password)> CreateUserAsync()
  {
    var userId = Interlocked.Increment(ref _userCounter);
    var username = $"testuser{userId}_{Guid.NewGuid().ToString("N")[..8]}";
    var email = $"{username}@test.com";
    var password = "TestPassword123!";

    var registerRequest = new
    {
      user = new
      {
        username,
        email,
        password,
      },
    };

    // Clone the client to avoid header conflicts
    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
    {
      Content = JsonContent.Create(registerRequest, options: _jsonOptions),
    };

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, _jsonOptions)!;
    return (userResponse.User.Token, username, email, password);
  }

  /// <summary>
  /// Creates an article for the authenticated user and returns the article slug and title.
  /// The fixture generates unique test data automatically.
  /// </summary>
  public async Task<(string Slug, string Title)> CreateArticleAsync(
    string token,
    string[]? tags = null)
  {
    var articleId = Interlocked.Increment(ref _articleCounter);
    var title = $"Test Article {articleId} - {Guid.NewGuid().ToString("N")[..8]}";
    var description = $"Test description for article {articleId}";
    var body = $"Test body content for article {articleId}";

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

    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/articles")
    {
      Content = JsonContent.Create(articleRequest, options: _jsonOptions),
    };
    request.Headers.Add("Authorization", $"Token {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var articleResponse = JsonSerializer.Deserialize<ArticleResponse>(responseContent, _jsonOptions)!;
    return (articleResponse.Article.Slug, articleResponse.Article.Title);
  }

  /// <summary>
  /// Creates multiple articles for the authenticated user.
  /// </summary>
  public async Task CreateArticlesAsync(string token, int count, string[]? tags = null)
  {
    for (var i = 0; i < count; i++)
    {
      await CreateArticleAsync(token, tags);
    }
  }

  /// <summary>
  /// Creates a comment on an article.
  /// </summary>
  public async Task CreateCommentAsync(string token, string articleSlug, string commentBody)
  {
    var commentRequest = new
    {
      comment = new
      {
        body = commentBody,
      },
    };

    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/articles/{articleSlug}/comments")
    {
      Content = JsonContent.Create(commentRequest, options: _jsonOptions),
    };
    request.Headers.Add("Authorization", $"Token {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Follows a user.
  /// </summary>
  public async Task FollowUserAsync(string followerToken, string usernameToFollow)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/profiles/{usernameToFollow}/follow");
    request.Headers.Add("Authorization", $"Token {followerToken}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Favorites an article.
  /// </summary>
  public async Task FavoriteArticleAsync(string token, string articleSlug)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/articles/{articleSlug}/favorite");
    request.Headers.Add("Authorization", $"Token {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Logs in a user and returns the authentication token.
  /// </summary>
  public async Task<string> LoginAsync(string email, string password)
  {
    var loginRequest = new
    {
      user = new
      {
        email,
        password,
      },
    };

    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/login")
    {
      Content = JsonContent.Create(loginRequest, options: _jsonOptions),
    };

    var response = await _httpClient.SendAsync(request);
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
