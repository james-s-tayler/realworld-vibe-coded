namespace E2eTests.Tests.RegisterPage;

/// <summary>
/// Happy path tests for the Registration page (/register).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task NewTenantSignup_ShowsUsersNavItem()
  {
    // Arrange
    var email = GenerateUniqueEmail(GenerateUniqueUsername("signup"));
    var password = "TestPassword123!";

    await Pages.RegisterPage.GoToAsync();

    // Act
    await Pages.RegisterPage.RegisterAsync(email, password);

    // Assert — first user is ADMIN, so Users nav should be visible
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    await Expect(Pages.UsersPage.UsersLink).ToBeVisibleAsync();

    // Verify navigation to Users page works
    await Pages.UsersPage.ClickUsersAsync();
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users");
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
  }
}
