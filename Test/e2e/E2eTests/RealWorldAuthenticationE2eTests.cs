using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class RealWorldAuthenticationE2eTests : ConduitPageTest
{
  [Fact]
  public async Task UserCanSignIn_WithExistingCredentials()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign In Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // First, register a user
      await RegisterUser();

      // Sign out - navigate to settings
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/settings", new() { Timeout = DefaultTimeout });
      await Page.GetByRole(AriaRole.Button, new() { Name = "Or click here to logout." }).ClickAsync();

      // Wait for sign in link to appear (indicates logged out state)
      await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" })).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Now sign in with the credentials
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });

      await Page.GetByPlaceholder("Email").FillAsync(TestEmail);
      await Page.GetByPlaceholder("Password").FillAsync(TestPassword);
      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

      // Verify redirect to home page
      await Page.WaitForURLAsync(BaseUrl, new() { Timeout = DefaultTimeout });

      // Verify user is logged in
      var userLink = Page.GetByRole(AriaRole.Link, new() { Name = TestUsername }).First;
      await userLink.WaitForAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await userLink.IsVisibleAsync(), "User should be logged in after sign in");
    }
    finally
    {
      await SaveTrace("sign_in_test");
    }
  }

  [Fact]
  public async Task UserCanSignOut()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "User Sign Out Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register and login a user
      await RegisterUser();

      // Navigate to settings
      await Page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/settings", new() { Timeout = DefaultTimeout });

      // Click logout button
      await Page.GetByRole(AriaRole.Button, new() { Name = "Or click here to logout." }).ClickAsync();

      // Verify user is logged out - Sign in and Sign up links should be visible
      var signInLink = Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" });
      await Expect(signInLink).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
      Assert.True(await signInLink.IsVisibleAsync(), "Sign in link should be visible after logout");

      var signUpLink = Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" });
      Assert.True(await signUpLink.IsVisibleAsync(), "Sign up link should be visible after logout");
    }
    finally
    {
      await SaveTrace("sign_out_test");
    }
  }

  [Fact]
  public async Task ProtectedRoutes_RedirectToLogin_WhenNotAuthenticated()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Protected Routes Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Try to access editor page without authentication
      await Page.GotoAsync($"{BaseUrl}/editor", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Should redirect to login
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);

      // Try to access settings page without authentication
      await Page.GotoAsync($"{BaseUrl}/settings", new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });

      // Should redirect to login
      await Page.WaitForURLAsync($"{BaseUrl}/login", new() { Timeout = DefaultTimeout });
      Assert.Contains("/login", Page.Url);
    }
    finally
    {
      await SaveTrace("protected_routes_test");
    }
  }
}
