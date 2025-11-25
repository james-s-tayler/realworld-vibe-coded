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

    var timestamp = DateTime.Now.Ticks;
    TestUsername = $"articleuser{timestamp}";
    TestEmail = $"articleuser{timestamp}@test.com";
    TestPassword = "TestPassword123!";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
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
