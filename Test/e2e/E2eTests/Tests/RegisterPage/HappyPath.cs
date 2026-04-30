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
  [TestCoverage(
    Id = "register-happy-001",
    FeatureArea = "auth",
    Behavior = "New tenant signup creates admin user who sees Users nav item",
    Verifies = ["Redirects to home page", "Users nav link visible", "Users page accessible"])]
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
