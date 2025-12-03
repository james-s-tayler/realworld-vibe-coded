namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Permission tests for the Profile page (/profile/:username).
/// </summary>
[Collection("E2E Tests")]
public class ProfilePagePermissionsTests : ConduitPageTest
{
  private string _testUsername1 = null!;
  private string _testEmail1 = null!;
  private string _testPassword1 = null!;

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    _testUsername1 = GenerateUniqueUsername("profileuser1");
    _testEmail1 = GenerateUniqueEmail(_testUsername1);
    _testPassword1 = "TestPassword123!";
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFavoritingArticleFromHomePage()
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

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
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
}
