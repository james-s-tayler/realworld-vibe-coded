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

    // Configure HttpClientHandler to accept self-signed certificates in test environments
    var handler = new HttpClientHandler();

    // WARNING: Disables all SSL/TLS certificate validation for E2E tests.
    // This is safe ONLY in isolated test environments (e.g., E2E tests with self-signed dev certificates in Docker containers).
    // NEVER use this pattern in production code, as it allows any certificate (including expired, self-signed, or malicious).
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

    _httpClient = new HttpClient(handler)
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
  /// Creates a user via API and returns the user credentials.
  /// The fixture generates unique test data automatically.
  /// This creates a new tenant for the user via /api/identity/register.
  /// </summary>
  public async Task<CreatedUser> CreateUserAsync()
  {
    var userId = Interlocked.Increment(ref _userCounter);
    var email = $"testuser{userId}_{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var password = "TestPassword123!";

    var registerRequest = new
    {
      email,
      password,
    };

    // Register via Identity - creates a new tenant
    using var registerHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/identity/register")
    {
      Content = JsonContent.Create(registerRequest, options: _jsonOptions),
    };

    var registerResponse = await _httpClient.SendAsync(registerHttpRequest);
    registerResponse.EnsureSuccessStatusCode();

    // Now login to get the token
    var token = await LoginAsync(email, password);

    return new CreatedUser
    {
      Token = token,
      Email = email,
      Password = password,
    };
  }

  /// <summary>
  /// Invites a user to an existing tenant via API and returns the user credentials.
  /// The invited user will belong to the same tenant as the inviting user.
  /// This uses /api/identity/invite with the inviting user's token.
  /// </summary>
  public async Task<CreatedUser> InviteUserAsync(string inviterToken)
  {
    var userId = Interlocked.Increment(ref _userCounter);
    var email = $"invited{userId}_{Guid.NewGuid().ToString("N")[..8]}@test.com";
    return await InviteUserAsync(inviterToken, email);
  }

  /// <summary>
  /// Invites a user with a specific email to an existing tenant via API and returns the user credentials.
  /// The invited user will belong to the same tenant as the inviting user.
  /// This uses /api/identity/invite with the inviting user's token.
  /// </summary>
  public async Task<CreatedUser> InviteUserAsync(string inviterToken, string email)
  {
    var password = "TestPassword123!";

    var inviteRequest = new
    {
      email,
      password,
    };

    // Invite via authenticated endpoint - creates user in same tenant
    using var inviteHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/identity/invite")
    {
      Content = JsonContent.Create(inviteRequest, options: _jsonOptions),
    };
    inviteHttpRequest.Headers.Add("Authorization", $"Bearer {inviterToken}");

    var inviteResponse = await _httpClient.SendAsync(inviteHttpRequest);
    inviteResponse.EnsureSuccessStatusCode();

    // Now login to get the token
    var token = await LoginAsync(email, password);

    return new CreatedUser
    {
      Token = token,
      Email = email,
      Password = password,
    };
  }

  /// <summary>
  /// Creates a user with all string fields at their maximum length via API.
  /// The fixture generates unique test data automatically.
  /// Note: Email is set to a reasonable length (100 chars) rather than max (255 chars)
  /// to avoid UI layout issues when email is used as username.
  /// </summary>
  public async Task<CreatedUser> CreateUserWithMaxLengthsAsync()
  {
    var userId = Interlocked.Increment(ref _userCounter);
    var guidPart = Guid.NewGuid().ToString("N")[..8];

    // Email: Use reasonable length (100 chars) to avoid UI overflow when used as username
    const int emailDomainLength = 9; // @test.com
    const int emailLength = 100; // Reasonable length instead of max 255
    const int emailLocalLength = emailLength - emailDomainLength; // 91 chars

    // Calculate how much padding we need for the email local part
    var emailBasePart = $"testuser{userId}_{guidPart}_";
    var emailPaddingLength = Math.Max(0, emailLocalLength - emailBasePart.Length);
    var emailLocalPart = emailPaddingLength > 0
        ? $"{emailBasePart}{new string('y', emailPaddingLength)}"
        : emailBasePart[..emailLocalLength]; // Truncate if needed

    var email = $"{emailLocalPart}@test.com";

    var password = "TestPassword123!";

    var registerRequest = new
    {
      email,
      password,
    };

    // Register via Identity - no token expected
    using var registerHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/identity/register")
    {
      Content = JsonContent.Create(registerRequest, options: _jsonOptions),
    };

    var registerResponse = await _httpClient.SendAsync(registerHttpRequest);
    registerResponse.EnsureSuccessStatusCode();

    // Now login to get the token
    var token = await LoginAsync(email, password);

    // Update user profile with max-length bio and image
    var bio = new string('B', 1000); // Bio: max 1000 chars
    var image = $"https://example.com/{new string('i', 476)}.jpg"; // Image URL: max 500 chars (20+476+4=500)

    await UpdateUserProfileAsync(token, bio, image);

    return new CreatedUser
    {
      Token = token,
      Email = email,
      Password = password,
    };
  }

  /// <summary>
  /// Creates an article for the authenticated user and returns the article details.
  /// The fixture generates unique test data automatically.
  /// </summary>
  public async Task<CreatedArticle> CreateArticleAsync(
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
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var articleResponse = JsonSerializer.Deserialize<ArticleResponse>(responseContent, _jsonOptions)!;
    return new CreatedArticle
    {
      Slug = articleResponse.Article.Slug,
      Title = articleResponse.Article.Title,
    };
  }

  /// <summary>
  /// Creates an article with custom field values for the authenticated user.
  /// </summary>
  public async Task<CreatedArticle> CreateArticleAsync(
    string token,
    string title,
    string description,
    string body,
    string[]? tags = null)
  {
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
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var articleResponse = JsonSerializer.Deserialize<ArticleResponse>(responseContent, _jsonOptions)!;
    return new CreatedArticle
    {
      Slug = articleResponse.Article.Slug,
      Title = articleResponse.Article.Title,
    };
  }

  /// <summary>
  /// Creates an article with maximum length fields (except body = 500 chars as specified).
  /// </summary>
  public async Task<CreatedArticle> CreateArticleWithMaxLengthsAsync(string token)
  {
    var articleId = Interlocked.Increment(ref _articleCounter);

    // Title: max 200 chars
    var title = $"Article{articleId} {new string('T', 200 - $"Article{articleId} ".Length)}";

    // Description: max 500 chars
    var description = new string('D', 500);

    // Body: 500 chars as specified in requirements
    var body = new string('B', 500);

    return await CreateArticleAsync(token, title, description, body);
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
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Creates a comment with maximum length body on an article.
  /// </summary>
  public async Task CreateCommentWithMaxLengthAsync(string token, string articleSlug)
  {
    // Comment body: max 5000 chars
    var commentBody = new string('C', 5000);
    await CreateCommentAsync(token, articleSlug, commentBody);
  }

  /// <summary>
  /// Follows a user.
  /// </summary>
  public async Task FollowUserAsync(string followerToken, string usernameToFollow)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/profiles/{usernameToFollow}/follow");
    request.Headers.Add("Authorization", $"Bearer {followerToken}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  /// <summary>
  /// Favorites an article.
  /// </summary>
  public async Task FavoriteArticleAsync(string token, string articleSlug)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/articles/{articleSlug}/favorite");
    request.Headers.Add("Authorization", $"Bearer {token}");

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
      email,
      password,
    };

    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identity/login?useCookies=false")
    {
      Content = JsonContent.Create(loginRequest, options: _jsonOptions),
    };

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var loginResponse = JsonSerializer.Deserialize<IdentityLoginResponse>(responseContent, _jsonOptions)!;
    return loginResponse.AccessToken;
  }

  /// <summary>
  /// Updates a user with optional fields and returns the updated user info.
  /// </summary>
  public async Task<UpdatedUser> UpdateUserAsync(
    string token,
    string? email = null,
    string? username = null,
    string? password = null,
    string? bio = null,
    string? image = null)
  {
    var userUpdate = new Dictionary<string, object?>();
    if (email != null)
    {
      userUpdate["email"] = email;
    }

    if (username != null)
    {
      userUpdate["username"] = username;
    }

    if (password != null)
    {
      userUpdate["password"] = password;
    }

    if (bio != null)
    {
      userUpdate["bio"] = bio;
    }

    if (image != null)
    {
      userUpdate["image"] = image;
    }

    var updateRequest = new
    {
      user = userUpdate,
    };

    using var request = new HttpRequestMessage(HttpMethod.Put, "/api/user")
    {
      Content = JsonContent.Create(updateRequest, options: _jsonOptions),
    };
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var userResponse = JsonSerializer.Deserialize<UserResponseWrapper>(responseContent, _jsonOptions)!;

    return new UpdatedUser
    {
      Email = userResponse.User.Email,
      Username = userResponse.User.Username,
      Bio = userResponse.User.Bio,
      Image = userResponse.User.Image,
    };
  }

  /// <summary>
  /// Updates a user's profile (bio and image).
  /// </summary>
  private async Task UpdateUserProfileAsync(string token, string bio, string image)
  {
    var updateRequest = new
    {
      user = new
      {
        bio,
        image,
      },
    };

    using var request = new HttpRequestMessage(HttpMethod.Put, "/api/user")
    {
      Content = JsonContent.Create(updateRequest, options: _jsonOptions),
    };
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
  }

  // DTOs for API responses
  private class IdentityLoginResponse
  {
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
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

  private class UserResponseWrapper
  {
    [JsonPropertyName("user")]
    public UserDataExtended User { get; set; } = null!;
  }

  private class UserDataExtended
  {
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("bio")]
    public string Bio { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }
  }
}
