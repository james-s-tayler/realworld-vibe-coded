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

    // Use unique identifiers with GUID to avoid collisions in parallel tests
    var uniqueId = GenerateUniqueId();
    TestUsername = $"testuser{uniqueId}";
    TestEmail = $"testuser{uniqueId}@test.com";
    TestPassword = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  public override async ValueTask DisposeAsync()
  {
    // Reset database after each test to clean up test data
    await ResetDatabaseAsync();

    await base.DisposeAsync();
  }

  /// <summary>
  /// Generates a unique identifier for test data by combining timestamp and a short GUID.
  /// This ensures uniqueness even when tests run in parallel.
  /// </summary>
  protected static string GenerateUniqueId(string prefix = "")
  {
    var timestamp = DateTime.UtcNow.Ticks;
    var shortGuid = Guid.NewGuid().ToString("N")[..8];
    return string.IsNullOrEmpty(prefix) ? $"{timestamp}{shortGuid}" : $"{prefix}{timestamp}{shortGuid}";
  }

  /// <summary>
  /// Generates a unique username for test purposes.
  /// </summary>
  protected static string GenerateUniqueUsername(string prefix = "user")
  {
    return GenerateUniqueId(prefix);
  }

  /// <summary>
  /// Generates a unique email for test purposes.
  /// </summary>
  protected static string GenerateUniqueEmail(string prefix = "user")
  {
    return $"{GenerateUniqueId(prefix)}@test.com";
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

  private static readonly HttpClient HttpClientInstance = new();

  /// <summary>
  /// Calls the dev-only endpoint to reset the database, clearing all test data.
  /// </summary>
  private async Task ResetDatabaseAsync()
  {
    var resetUrl = $"{BaseUrl}/api/dev-only/test-data/reset";
    await HttpClientInstance.DeleteAsync(resetUrl);
  }
}
