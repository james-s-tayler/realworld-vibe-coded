namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Permission tests for the Profile page (/profile/:username).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
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
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    await CreateArticleAsync();

    await SignOutAsync();

    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickGlobalFeedTabAsync();

    // Act
    await homePage.ClickFavoriteButtonOnPreviewAsync();

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }

  [Fact]
  public async Task UnauthenticatedUser_RedirectsToLogin_WhenFollowingUser()
  {
    // Arrange
    await RegisterUserAsync(_testUsername1, _testEmail1, _testPassword1);
    await CreateArticleAsync();

    await SignOutAsync();

    var profilePage = GetProfilePage();
    await profilePage.GoToAsync(_testUsername1);
    await profilePage.WaitForProfileToLoadAsync(_testUsername1);

    // Act
    await profilePage.ClickFollowButtonWithoutWaitAsync(_testUsername1);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }
}
