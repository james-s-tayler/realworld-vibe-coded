using Microsoft.Playwright;

namespace E2eTests.Tests.Demo;

/// <summary>
/// Demonstrates the starter template features end-to-end.
/// This test walks through: register, login, dashboard, settings, profile,
/// and user management (invite).
/// </summary>
public class StarterTemplateDemo : AppPageTest
{
  private static readonly string TracePath = Path.Combine(
    Constants.ReportsTestE2eArtifacts,
    $"StarterTemplateDemo_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

  public StarterTemplateDemo(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  public override async ValueTask DisposeAsync()
  {
    if (!Directory.Exists(Constants.ReportsTestE2eArtifacts))
    {
      Directory.CreateDirectory(Constants.ReportsTestE2eArtifacts);
    }

    // Always save trace (base class only saves on failure)
    await Context.Tracing.StopAsync(new() { Path = TracePath });

    // Base class will try to stop tracing again (no-op since already stopped)
    // but still need it for browser context cleanup
    try
    {
      await base.DisposeAsync();
    }
    catch (PlaywrightException)
    {
      // Tracing already stopped — ignore
    }
  }

  [Fact]
  public async Task FullStarterTemplateWalkthrough()
  {
    var uniqueId = Guid.NewGuid().ToString("N")[..8];
    var adminEmail = $"admin-{uniqueId}@demo.com";
    var adminPassword = "TestPassword123!";

    // === STEP 1: Register a new user (first user becomes admin) ===
    await Pages.RegisterPage.GoToAsync();
    await Pages.RegisterPage.EmailInput.FillAsync(adminEmail);
    await Pages.RegisterPage.PasswordInput.FillAsync(adminPassword);
    await Pages.RegisterPage.ClickSignUpButtonAsync();

    // Registration auto-logs in and redirects to dashboard
    await Expect(Pages.LoginPage.SettingsLink.First).ToBeVisibleAsync();

    // === STEP 2: Verify the Dashboard is shown ===
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome" })).ToBeVisibleAsync();

    // === STEP 3: Navigate to Settings and update profile ===
    await Pages.SettingsPage.GoToAsync();
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/settings");

    // Update the username
    var adminUsername = $"admin-demo-{uniqueId}";
    await Pages.SettingsPage.UsernameInput.ClearAsync();
    await Pages.SettingsPage.UsernameInput.FillAsync(adminUsername);
    await Pages.SettingsPage.BioInput.FillAsync("I am the admin");
    await Pages.SettingsPage.ClickUpdateSettingsButtonAsync();
    await Pages.SettingsPage.VerifySuccessMessageAsync();

    // === STEP 4: Navigate to own profile ===
    await Page.GotoAsync($"{BaseUrl}/profile/{adminUsername}");
    await Pages.ProfilePage.VerifyProfileHeadingAsync(adminUsername);
    await Pages.ProfilePage.VerifyBioVisibleAsync("I am the admin");

    // === STEP 5: Navigate to Users page (admin only) ===
    await Pages.UsersPage.GoToAsync();
    await Expect(Pages.UsersPage.Heading).ToBeVisibleAsync();
    await Expect(Pages.UsersPage.InviteUserButton).ToBeVisibleAsync();

    // === STEP 6: Invite a second user ===
    var invitedEmail = $"invited-{uniqueId}@demo.com";
    var invitedPassword = "InvitedPass123!";
    await Pages.UsersPage.InviteUserAsync(invitedEmail, invitedPassword);
    await Pages.UsersPage.VerifyUserVisibleAsync(invitedEmail);

    // === STEP 7: Logout ===
    await Pages.SettingsPage.GoToAsync();
    await Pages.SettingsPage.LogoutAsync();
    await Expect(Pages.SettingsPage.SignInLink).ToBeVisibleAsync();

    // === STEP 8: Login as the invited user ===
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.FillLoginFormAsync(invitedEmail, invitedPassword);
    await Pages.LoginPage.ClickSignInButtonAsync();
    await Expect(Pages.LoginPage.SettingsLink.First).ToBeVisibleAsync();

    // === STEP 9: Verify invited user sees dashboard ===
    await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome" })).ToBeVisibleAsync();

    // === STEP 10: Invited user views admin's profile ===
    await Page.GotoAsync($"{BaseUrl}/profile/{adminUsername}");
    await Pages.ProfilePage.VerifyProfileHeadingAsync(adminUsername);
    await Pages.ProfilePage.VerifyBioVisibleAsync("I am the admin");

    // === STEP 11: Verify invited user does NOT see Users link (non-admin) ===
    await Expect(Page.GetByLabel("Main navigation").GetByRole(AriaRole.Link, new() { Name = "Users", Exact = true })).Not.ToBeVisibleAsync();
  }
}
