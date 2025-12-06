namespace E2eTests.Tests.ProfilePage;
using static E2eTests.PageModels.Pages;

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

    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Act
    await Pages.HomePage.ClickFavoriteButtonOnPreviewAsync();

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

    await Pages.ProfilePage.GoToAsync(_testUsername1);
    await Pages.ProfilePage.WaitForProfileToLoadAsync(_testUsername1);

    // Act
    await Pages.ProfilePage.ClickFollowButtonWithoutWaitAsync(_testUsername1);

    // Assert
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
  }
}
