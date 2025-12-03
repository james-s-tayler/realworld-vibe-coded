using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2eTests.Tests;

/// <summary>
/// Tests for the Profile page (/profile/:username) including following, My Articles, and Favorited Articles.
/// </summary>
[Collection("E2E Tests")]
public class ProfilePageTests : ConduitPageTest
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
    await Context.Tracing.StartAsync(new()
    {
      Title = "View Other User Profile Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Register second user
      await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

      // Navigate directly to first user's profile using page model
      var profilePage = GetProfilePage();
      await profilePage.GoToAsync(_testUsername1);

      // Verify profile information is displayed
      await profilePage.VerifyProfileHeadingAsync(_testUsername1);
      await profilePage.VerifyMyArticlesTabVisibleAsync();
    }
    finally
    {
      await SaveTrace("view_other_profile_test");
    }
  }

  [Fact]
  public async Task UserCanFollowAndUnfollowOtherUser()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Follow User Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Register second user
      await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to first user's profile using page model
      var profilePage = GetProfilePage();
      await profilePage.GoToAsync(_testUsername1);

      // Follow and unfollow using page model
      await profilePage.ClickFollowButtonAsync(_testUsername1);
      await profilePage.ClickUnfollowButtonAsync(_testUsername1);
    }
    finally
    {
      await SaveTrace("follow_user_test");
    }
  }

  [Fact]
  public async Task UserCanViewFavoritedArticlesOnProfile()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Favorited Articles on Profile Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register first user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      var (_, articleTitle) = await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Register second user
      await RegisterUserAsync(_testUsername2, _testEmail2, _testPassword2);

      // Navigate to the first user's article
      var homePage = GetHomePage();
      await homePage.GoToAsync();
      await homePage.ClickGlobalFeedTabAsync();

      // Click on the article and favorite it
      var articlePage = await homePage.ClickArticleAsync(articleTitle);
      await articlePage.ClickFavoriteButtonAsync();

      // Navigate to own profile
      var profilePage = GetProfilePage();
      await profilePage.GoToAsync(_testUsername2);

      // Click on Favorited Articles tab
      await profilePage.ClickFavoritedArticlesTabAsync();

      // Verify favorited article is visible
      await profilePage.VerifyArticleVisibleAsync(articleTitle);
    }
    finally
    {
      await SaveTrace("favorited_articles_profile_test");
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

      // Navigate to profile page using page model
      var profilePage = GetProfilePage();
      await profilePage.GoToAsync(username);

      // Wait for My Articles tab to be visible and articles to load
      await profilePage.WaitForArticlesToLoadAsync();
      await profilePage.VerifyArticleCountAsync(20);

      // Verify pagination control is visible
      await profilePage.VerifyPaginationVisibleAsync();

      // Navigate through pages
      await profilePage.ClickNextPageAsync();
      await profilePage.ClickNextPageAsync();
      await profilePage.VerifyArticleCountAsync(10);

      // Navigate backward
      await profilePage.ClickPreviousPageAsync();
      await profilePage.VerifyArticleCountAsync(20);
    }
    finally
    {
      await SaveTrace("profile_my_articles_pagination_test");
    }
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticleFromHomePage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Favorite From Home Page Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Navigate to the home page
      var homePage = GetHomePage();
      await homePage.GoToAsync();
      await homePage.ClickGlobalFeedTabAsync();

      // Click the favorite button on the article preview
      await homePage.ClickFavoriteButtonOnPreviewAsync();

      // Verify redirect to login page
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_favorite_home_page_redirect_test");
    }
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Unauthenticated Follow User Redirect Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user and create an article
      await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
      await CreateArticleAsync();

      // Sign out
      await SignOutAsync();

      // Navigate to the user's profile
      var profilePage = GetProfilePage();
      await profilePage.GoToAsync(_testUsername1);
      await profilePage.WaitForProfileToLoadAsync(_testUsername1);

      // Click the follow button (unauthenticated)
      await profilePage.ClickFollowButtonWithoutWaitAsync(_testUsername1);

      // Verify redirect to login page
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("unauthenticated_follow_redirect_test");
    }
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
