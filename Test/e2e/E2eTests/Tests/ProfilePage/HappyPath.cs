namespace E2eTests.Tests.ProfilePage;

/// <summary>
/// Happy path tests for the Profile page (/profile/:username).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanViewOwnProfile()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    // Act
    await Pages.ProfilePage.GoToAsync(user.Email);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(user.Email);
  }

  [Fact]
  public async Task UserCanViewOtherUsersProfile()
  {
    // Arrange
    var user1 = await Api.CreateUserAsync();
    var user2 = await Api.InviteUserAsync(user1.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user2.Email, user2.Password);

    // Act
    await Pages.ProfilePage.GoToAsync(user1.Email);

    // Assert
    await Pages.ProfilePage.VerifyProfileHeadingAsync(user1.Email);
  }
}
