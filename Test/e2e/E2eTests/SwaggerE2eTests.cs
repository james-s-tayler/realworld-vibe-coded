using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace E2eTests;

[Collection("E2E Tests")]
public class SwaggerE2eTests : PageTest
{
  public override BrowserNewContextOptions ContextOptions()
  {
    return new BrowserNewContextOptions()
    {
      IgnoreHTTPSErrors = true
    };
  }

  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Configure additional browser options to ignore HTTPS errors
    await Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
    {
      ["User-Agent"] = "E2E-Test-Suite"
    });

    // Start tracing
    await Context.Tracing.StartAsync(new()
    {
      Title = "Swagger API Docs Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true
    });

    try
    {
      // Use environment variable for URL if available (for Docker), otherwise use localhost
      var baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";
      var swaggerUrl = $"{baseUrl}/swagger/index.html";

      // Navigate to Swagger docs
      await Page.GotoAsync(swaggerUrl, new()
      {
        WaitUntil = WaitUntilState.NetworkIdle,
        Timeout = 30000
      });

      // Wait for Swagger UI to load and verify it's visible (use first occurrence)
      var swaggerContainer = Page.Locator(".swagger-ui").First;
      await swaggerContainer.WaitForAsync(new() { Timeout = 10000 });

      var isVisible = await swaggerContainer.IsVisibleAsync();
      Assert.True(isVisible, "Swagger UI container should be visible");

      // Verify that the API title is displayed 
      var apiInfo = Page.Locator(".info .title");
      await apiInfo.WaitForAsync(new() { Timeout = 10000 });
      var title = await apiInfo.TextContentAsync();
      Assert.NotNull(title);
      Assert.NotEmpty(title.Trim());

      // Verify basic functionality - just that Swagger loaded successfully
      var hasSchemaSection = await Page.Locator(".scheme-container").IsVisibleAsync();
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
        Path = Path.Combine(Constants.TracesDirectory, $"swagger_test_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip")
      });
    }
  }
}
