using E2eTests.PageModels;
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

    // Start Playwright tracing automatically for every test
    await StartTracingAsync();

    BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

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

  // Page model factory methods
  protected LoginPage GetLoginPage() => new(Page, BaseUrl);

  protected RegisterPage GetRegisterPage() => new(Page, BaseUrl);

  protected HomePage GetHomePage() => new(Page, BaseUrl);

  protected EditorPage GetEditorPage() => new(Page, BaseUrl);

  protected ArticlePage GetArticlePage() => new(Page, BaseUrl);

  protected ProfilePage GetProfilePage() => new(Page, BaseUrl);

  protected SettingsPage GetSettingsPage() => new(Page, BaseUrl);

  protected SwaggerPage GetSwaggerPage() => new(Page, BaseUrl);

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
      // Log but don't fail the test if wipe fails.
      Console.WriteLine($"Warning: Failed to wipe test data: {ex.Message}");
    }
  }

  /// <summary>
  /// Registers a user using the default test credentials and returns the HomePage.
  /// </summary>
  protected async Task<HomePage> RegisterUserAsync()
  {
    return await RegisterUserAsync(TestUsername, TestEmail, TestPassword);
  }

  /// <summary>
  /// Registers a user with specified credentials and returns the HomePage.
  /// </summary>
  protected async Task<HomePage> RegisterUserAsync(string username, string email, string password)
  {
    var homePage = GetHomePage();
    await homePage.GoToAsync();
    await homePage.ClickSignUpAsync();

    var registerPage = GetRegisterPage();
    return await registerPage.RegisterAsync(username, email, password);
  }

  /// <summary>
  /// Signs out the current user via the settings page.
  /// </summary>
  protected async Task SignOutAsync()
  {
    var settingsPage = GetSettingsPage();
    await settingsPage.GoToAsync();
    await settingsPage.LogoutAsync();
  }

  /// <summary>
  /// Creates a new article and returns the ArticlePage.
  /// </summary>
  protected async Task<(ArticlePage ArticlePage, string Title)> CreateArticleAsync()
  {
    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var homePage = GetHomePage();
    await homePage.ClickNewArticleAsync();

    var editorPage = GetEditorPage();
    var articlePage = await editorPage.CreateArticleAsync(articleTitle, "Test article for E2E testing", "This is a test article body.");
    return (articlePage, articleTitle);
  }

  /// <summary>
  /// Creates a new article with a specific tag and returns the ArticlePage.
  /// </summary>
  protected async Task<(ArticlePage ArticlePage, string Title)> CreateArticleWithTagAsync(string tag)
  {
    var articleTitle = $"Tagged Article {GenerateUniqueUsername("tag")}";
    var homePage = GetHomePage();
    await homePage.ClickNewArticleAsync();

    var editorPage = GetEditorPage();
    var articlePage = await editorPage.CreateArticleWithTagsAsync(articleTitle, "Test article with tag", "This is a test article body.", tag);
    return (articlePage, articleTitle);
  }

  // Legacy methods for backward compatibility during migration
  protected async Task RegisterUser()
  {
    await RegisterUserAsync(TestUsername, TestEmail, TestPassword);
  }

  protected async Task RegisterUser(string username, string email, string password)
  {
    await RegisterUserAsync(username, email, password);
  }

  protected async Task SignOut()
  {
    await SignOutAsync();
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
    var testFailed = testState?.Result == Xunit.TestResult.Failed;

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
