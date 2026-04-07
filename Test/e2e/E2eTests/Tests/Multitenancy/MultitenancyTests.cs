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
