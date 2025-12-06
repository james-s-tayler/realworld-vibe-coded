using E2eTests.PageModels;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

public abstract class AppPageTest : PageTest
{
  protected const int DefaultTimeout = 10000;
  protected string BaseUrl = null!;
  protected string TestUsername = null!;
  protected string TestEmail = null!;
  protected string TestPassword = null!;
  protected PageObjects Pages = null!;

  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true,
    };
  }

  public override async ValueTask InitializeAsync()
  {
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_DOCKER") == null)
    {
      Environment.SetEnvironmentVariable("HEADED", "1");
    }

    await base.InitializeAsync();

    // Start Playwright tracing automatically for every test
    await StartTracingAsync();

    BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    // Initialize the Pages API
    Pages = new PageObjects(Page, BaseUrl);

    // Wipe all test data BEFORE each test to ensure a clean slate
    await WipeTestData();

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
    // Stop Playwright tracing and save the trace file
    await StopTracingAsync();

    // Also wipe after each test for cleanup (optional but helps with debugging)
    await WipeTestData();
    await base.DisposeAsync();
  }

  protected static string GenerateUniqueUsername(string prefix = "user")
  {
    var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 chars of GUID for shorter usernames
    return $"{prefix}{guid}";
  }

  protected static string GenerateUniqueEmail(string username)
  {
    var guid = Guid.NewGuid().ToString("N")[..8];
    return $"{username}{guid}@test.com";
  }



  /// <summary>
  /// Registers a user using the default test credentials.
  /// </summary>
  protected async Task RegisterUserAsync()
  {
    await RegisterUserAsync(TestUsername, TestEmail, TestPassword);
  }

  /// <summary>
  /// Registers a user with specified credentials.
  /// </summary>
  protected async Task RegisterUserAsync(string username, string email, string password)
  {
    await Pages.HomePage.GoToAsync();
    await Pages.HomePage.ClickSignUpAsync();
    await Pages.RegisterPage.RegisterAsync(username, email, password);
  }

  /// <summary>
  /// Signs out the current user via the settings page.
  /// </summary>
  protected async Task SignOutAsync()
  {
    await Pages.SettingsPage.GoToAsync();
    await Pages.SettingsPage.LogoutAsync();
  }

  /// <summary>
  /// Creates a new article and returns the article title.
  /// </summary>
  protected async Task<string> CreateArticleAsync()
  {
    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    await Pages.HomePage.ClickNewArticleAsync();
    await Pages.EditorPage.CreateArticleAsync(articleTitle, "Test article for E2E testing", "This is a test article body.");
    return articleTitle;
  }

  /// <summary>
  /// Creates a new article with a specific tag and returns the article title.
  /// </summary>
  protected async Task<string> CreateArticleWithTagAsync(string tag)
  {
    var articleTitle = $"Tagged Article {GenerateUniqueUsername("tag")}";
    await Pages.HomePage.ClickNewArticleAsync();
    await Pages.EditorPage.CreateArticleWithTagsAsync(articleTitle, "Test article with tag", "This is a test article body.", tag);
    return articleTitle;
  }

  /// <summary>
  /// Gets the current test name from xUnit's TestContext.
  /// </summary>
  private static string GetTestName()
  {
    var testDisplayName = TestContext.Current.Test?.TestDisplayName;
    if (string.IsNullOrEmpty(testDisplayName))
    {
      return "unknown_test";
    }

    // Sanitize the test name to be safe for file names
    var sanitized = testDisplayName
      .Replace(" ", "_")
      .Replace("(", "_")
      .Replace(")", "_")
      .Replace(",", "_")
      .Replace("\"", string.Empty)
      .Replace("'", string.Empty)
      .Replace(":", "_")
      .Replace("/", "_")
      .Replace("\\", "_");

    return sanitized;
  }

  /// <summary>
  /// Wipes all users and user-generated content from the database.
  /// Called after each test to ensure test isolation.
  /// </summary>
  private async Task WipeTestData()
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
      // Log but don't fail the test if wipe fails.
      Console.WriteLine($"Warning: Failed to wipe test data: {ex.Message}");
    }
  }

  /// <summary>
  /// Starts Playwright tracing for the current test.
  /// </summary>
  private async Task StartTracingAsync()
  {
    var testName = GetTestName();
    await Context.Tracing.StartAsync(new()
    {
      Title = testName,
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });
  }

  /// <summary>
  /// Stops Playwright tracing and saves the trace file only if the test failed.
  /// </summary>
  private async Task StopTracingAsync()
  {
    // Check if the test failed - only save trace for failed tests
    var testState = TestContext.Current.TestState;
    var testFailed = testState?.Result == TestResult.Failed;

    if (testFailed)
    {
      if (!Directory.Exists(Constants.TracesDirectory))
      {
        Directory.CreateDirectory(Constants.TracesDirectory);
      }

      var testName = GetTestName();
      await Context.Tracing.StopAsync(new()
      {
        Path = Path.Combine(Constants.TracesDirectory, $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
      });
    }
    else
    {
      // Discard the trace for passed tests (don't save to disk)
      await Context.Tracing.StopAsync();
    }
  }
}
