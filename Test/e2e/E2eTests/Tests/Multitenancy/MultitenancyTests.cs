namespace E2eTests.Tests.Multitenancy;

/// <summary>
/// Tests for multitenancy data isolation and duplicate handling.
/// These tests verify that tenants are properly isolated and that duplicates are allowed across tenants.
/// </summary>
public class MultitenancyTests : AppPageTest
{
  public MultitenancyTests(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task Articles_AreIsolated_BetweenTenants()
  {
    // Arrange - create two separate tenants with users
    var tenant1User = await Api.CreateUserAsync(); // Creates tenant 1
    var tenant2User = await Api.CreateUserAsync(); // Creates tenant 2

    // Create an article in tenant 1
    var articleTitle = $"Tenant 1 Article {Guid.NewGuid().ToString("N")[..8]}";
    var articleInTenant1 = await Api.CreateArticleAsync(
      tenant1User.Token,
      articleTitle,
      "Description for tenant 1",
      "Body for tenant 1",
      new[] { "tenant1" });

    // Act - login as tenant 2 user and navigate to home page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant2User.Email, tenant2User.Password);
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();

    // Assert - article from tenant 1 should not be visible to tenant 2
    await Expect(Page.GetByText(articleTitle)).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task Tags_AreIsolated_BetweenTenants()
  {
    // Arrange - create two separate tenants with users
    var tenant1User = await Api.CreateUserAsync(); // Creates tenant 1
    var tenant2User = await Api.CreateUserAsync(); // Creates tenant 2

    // Create an article with unique tags in tenant 1
    var uniqueTag = $"tenant1tag{Guid.NewGuid().ToString("N")[..8]}";
    await Api.CreateArticleAsync(
      tenant1User.Token,
      "Article with unique tags",
      "Description",
      "Body",
      new[] { uniqueTag, "anothertag" });

    // Act - login as tenant 2 user and navigate to home page
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant2User.Email, tenant2User.Password);
    await Pages.HomePage.GoToAsync();

    // Assert - tags from tenant 1 should not be visible in tenant 2's sidebar
    await Expect(Page.GetByText(uniqueTag)).Not.ToBeVisibleAsync();
  }

  [Fact]
  public async Task Users_AreIsolated_BetweenTenants()
  {
    // Arrange - create two separate tenants with users
    var tenant1User = await Api.CreateUserAsync(); // Creates tenant 1
    var tenant2User = await Api.CreateUserAsync(); // Creates tenant 2

    // Act - login as tenant 2 user and try to navigate to tenant 1 user's profile
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant2User.Email, tenant2User.Password);

    // Try to access tenant 1 user's profile directly via URL
    await Page.GotoAsync($"{BaseUrl}/profile/{tenant1User.Email}");

    // Assert - the page should show an error message indicating the user was not found
    // In a multitenancy context, profiles from other tenants should not be accessible
    await Expect(Page.Locator("body")).ToContainTextAsync("was not found");
  }

  [Fact]
  public async Task DuplicateSlugs_AreAllowed_BetweenTenants()
  {
    // Arrange - create two separate tenants with users
    var tenant1User = await Api.CreateUserAsync(); // Creates tenant 1
    var tenant2User = await Api.CreateUserAsync(); // Creates tenant 2

    // Create an article with the same title in both tenants
    var sameTitle = $"Same Article Title {Guid.NewGuid().ToString("N")[..8]}";
    var article1 = await Api.CreateArticleAsync(
      tenant1User.Token,
      sameTitle,
      "Description 1",
      "Body 1");

    // Act - create the same article in tenant 2 (should succeed)
    var article2 = await Api.CreateArticleAsync(
      tenant2User.Token,
      sameTitle,
      "Description 2",
      "Body 2");

    // Assert - both articles should have the same slug (but exist in different tenants)
    if (article1.Slug != article2.Slug)
    {
      throw new InvalidOperationException($"Expected slugs to match. Tenant 1: {article1.Slug}, Tenant 2: {article2.Slug}");
    }

    // Verify tenant 1 can access their article
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant1User.Email, tenant1User.Password);
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleVisibleAsync(article1.Title);

    // Verify tenant 2 can access their article
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant2User.Email, tenant2User.Password);
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickGlobalFeedTabAsync();
    await Pages.HomePage.VerifyArticleVisibleAsync(article2.Title);
  }

  [Fact]
  public async Task DuplicateUsernames_AreAllowed_BetweenTenants()
  {
    // Arrange - create two separate tenants with users
    var tenant1User = await Api.CreateUserAsync(); // Creates tenant 1
    var tenant2User = await Api.CreateUserAsync(); // Creates tenant 2

    // Both users initially use their email as username
    // We'll update both to use the same custom username
    var sharedUsername = $"shared-user-{Guid.NewGuid().ToString("N")[..8]}";

    // Act - login as tenant 1 user and update username
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant1User.Email, tenant1User.Password);
    await Pages.SettingsPage.GoToAsync();
    await Pages.SettingsPage.UsernameInput.ClearAsync();
    await Pages.SettingsPage.UsernameInput.FillAsync(sharedUsername);
    await Pages.SettingsPage.ClickUpdateSettingsButtonAsync();
    await Pages.SettingsPage.VerifySuccessMessageAsync();

    // Now login as tenant 2 user and update to the same username (should succeed)
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant2User.Email, tenant2User.Password);
    await Pages.SettingsPage.GoToAsync();
    await Pages.SettingsPage.UsernameInput.ClearAsync();
    await Pages.SettingsPage.UsernameInput.FillAsync(sharedUsername);
    await Pages.SettingsPage.ClickUpdateSettingsButtonAsync();
    await Pages.SettingsPage.VerifySuccessMessageAsync();

    // Assert - verify the update was successful by checking the profile page
    await Page.GotoAsync($"{BaseUrl}/profile/{sharedUsername}");
    await Pages.ProfilePage.VerifyProfileHeadingAsync(sharedUsername);

    // Verify tenant 1 user still has the same username
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(tenant1User.Email, tenant1User.Password);
    await Page.GotoAsync($"{BaseUrl}/profile/{sharedUsername}");
    await Pages.ProfilePage.VerifyProfileHeadingAsync(sharedUsername);
  }
}
