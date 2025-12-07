using E2eTests.PageModels;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using SixLabors.ImageSharp;

namespace E2eTests;

public abstract class AppPageTest : PageTest
{
  protected const int DefaultTimeout = 10000;
  protected string BaseUrl = null!;
  protected string TestUsername = null!;
  protected string TestEmail = null!;
  protected string TestPassword = null!;
  protected PageObjects Pages = null!;
  protected ApiFixture Api = null!;

  protected AppPageTest(ApiFixture apiFixture)
  {
    Api = apiFixture;
  }

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
  /// Takes a screenshot of the current page and saves it to the artifacts directory.
  /// The screenshot filename will match the current executing test name.
  /// Returns the path to the saved screenshot file.
  /// </summary>
  protected async Task<string> TakeScreenshotAsync()
  {
    if (!Directory.Exists(Constants.ReportsTestE2eArtifacts))
    {
      Directory.CreateDirectory(Constants.ReportsTestE2eArtifacts);
    }

    var testName = GetTestName();
    var screenshotPath = Path.Combine(Constants.ReportsTestE2eArtifacts, $"{testName}_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

    await Page.ScreenshotAsync(new()
    {
      Path = screenshotPath,
      FullPage = true,
    });

    return screenshotPath;
  }

  /// <summary>
  /// Asserts that the screenshot width does not exceed the viewport width.
  /// This helps catch layout bugs where content overflows horizontally.
  /// </summary>
  protected async Task AssertScreenshotWidthNotExceedingViewportAsync(string screenshotPath)
  {
    // Get viewport width
    var viewportSize = Page.ViewportSize;
    if (viewportSize == null)
    {
      throw new InvalidOperationException("Viewport size is not available");
    }

    // Read screenshot dimensions using SixLabors.ImageSharp
    using var image = await Image.LoadAsync(screenshotPath);
    var screenshotWidth = image.Width;

    // Assert screenshot width does not exceed viewport width
    if (screenshotWidth > viewportSize.Width)
    {
      throw new InvalidOperationException(
        $"Screenshot width ({screenshotWidth}px) exceeds viewport width ({viewportSize.Width}px). This indicates horizontal overflow/layout issues.");
    }
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
      if (!Directory.Exists(Constants.ReportsTestE2eArtifacts))
      {
        Directory.CreateDirectory(Constants.ReportsTestE2eArtifacts);
      }

      var testName = GetTestName();
      await Context.Tracing.StopAsync(new()
      {
        Path = Path.Combine(Constants.ReportsTestE2eArtifacts, $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
      });
    }
    else
    {
      // Discard the trace for passed tests (don't save to disk)
      await Context.Tracing.StopAsync();
    }
  }
}
