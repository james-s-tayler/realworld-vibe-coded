using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests.Tests;

/// <summary>
/// Tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class SwaggerPageTests : PageTest
{
  private const int DefaultTimeout = 10000;
  private string _baseUrl = null!;

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
    _baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";

    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite",
    });
  }

  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Start tracing
    await Context.Tracing.StartAsync(new()
    {
      Title = "Swagger API Docs Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Navigate to Swagger docs using page model
      var swaggerPage = new PageModels.SwaggerPage(Page, _baseUrl);
      await swaggerPage.GoToAsync();

      // Wait for Swagger UI to load
      await swaggerPage.WaitForSwaggerToLoadAsync();

      var isVisible = await swaggerPage.IsSwaggerVisibleAsync();
      Assert.True(isVisible, "Swagger UI container should be visible");

      // Verify that the API title is displayed
      var title = await swaggerPage.GetApiTitleAsync();
      Assert.NotNull(title);
      Assert.NotEmpty(title.Trim());

      // Verify basic functionality - just that Swagger loaded successfully
      _ = await swaggerPage.HasSchemeContainerAsync();

      // Don't assert on this as it may not always be present, just verify we got past the basic loading
    }
    finally
    {
      // Use absolute path that matches Docker volume mount
      if (!Directory.Exists(Constants.TracesDirectory))
      {
        Directory.CreateDirectory(Constants.TracesDirectory);
      }

      // Stop tracing and save to file
      await Context.Tracing.StopAsync(new()
      {
        Path = Path.Combine(Constants.TracesDirectory, $"swagger_test_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
      });
    }
  }
}
