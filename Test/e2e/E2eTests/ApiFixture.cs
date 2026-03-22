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
}
