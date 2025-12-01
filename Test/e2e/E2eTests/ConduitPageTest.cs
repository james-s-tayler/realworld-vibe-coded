using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

public abstract class ConduitPageTest : PageTest
{
  protected const int DefaultTimeout = 10000;
  protected string BaseUrl = null!;
  protected string TestUsername = null!;
  protected string TestEmail = null!;
  protected string TestPassword = null!;

  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true,
    };
  }

  public override async ValueTask InitializeAsync()
  {
    await base.InitializeAsync();

    BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    // Generate unique test user credentials
    TestUsername = GenerateUniqueUsername("articleuser");
    TestEmail = GenerateUniqueEmail(TestUsername);
    TestPassword = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  public override async ValueTask DisposeAsync()
  {
    // Wipe all test data after each test to ensure isolation
    await WipeTestData();
    await base.DisposeAsync();
  }

  /// <summary>
  /// Generates a unique username with an optional prefix.
  /// Uses a short random suffix to ensure uniqueness while keeping username short.
  /// </summary>
  protected static string GenerateUniqueUsername(string prefix = "user")
  {
    var random = Random.Shared.Next(10000, 99999);
    return $"{prefix}{random}";
  }

  /// <summary>
  /// Generates a unique email based on a username.
  /// </summary>
  protected static string GenerateUniqueEmail(string username)
  {
    return $"{username}@test.com";
  }

  /// <summary>
  /// Wipes all users and user-generated content from the database.
  /// Called after each test to ensure test isolation.
  /// </summary>
  protected async Task WipeTestData()
  {
    try
    {
      var apiContext = await Playwright.APIRequest.NewContextAsync(new()
      {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
      });

      await apiContext.DeleteAsync("/dev-only/test-data/wipe");
      await apiContext.DisposeAsync();
    }
    catch (Exception ex)
    {
      // Log but don't fail the test if wipe fails
      Console.WriteLine($"Warning: Failed to wipe test data: {ex.Message}");
    }
  }

  protected async Task RegisterUser()
  {
    await RegisterUser(TestUsername, TestEmail, TestPassword);
  }

  protected async Task RegisterUser(string username, string email, string password)
  {
    await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
    await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/register", new() { Timeout = DefaultTimeout });

    // Fill in the form
    await Page.GetByPlaceholder("Username").FillAsync(username);
    await Page.GetByPlaceholder("Email").FillAsync(email);
    await Page.GetByPlaceholder("Password").FillAsync(password);

    await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

    // Wait for the user link to appear in the header to confirm login completed
    await Page.GetByRole(AriaRole.Link, new() { Name = username }).First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });
  }

  protected async Task SignOut()
  {
    await Page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).ClickAsync();
    await Page.WaitForURLAsync($"{BaseUrl}/settings", new() { Timeout = DefaultTimeout });

    // The logout button contains "Or click here to logout."
    await Page.GetByRole(AriaRole.Button, new() { Name = "Or click here to logout." }).ClickAsync();

    // After logout, the app navigates to login page
    await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" })).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  protected async Task SaveTrace(string testName)
  {
    if (!Directory.Exists(Constants.TracesDirectory))
    {
      Directory.CreateDirectory(Constants.TracesDirectory);
    }

    await Context.Tracing.StopAsync(new()
    {
      Path = Path.Combine(Constants.TracesDirectory, $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
    });
  }
}
