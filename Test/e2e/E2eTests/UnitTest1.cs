using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace E2eTests;

[CollectionDefinition("browser-tests", DisableParallelization = true)]
public class BrowserCollectionDefinition { }

[Collection("browser-tests")]
public class IntegrationTests : PlaywrightTest
{
    [Fact]
    public async Task Should_Load_Static_Html_Page()
    {
        // Create a simple HTML file to test Playwright integration
        var tempHtml = Path.GetTempFileName() + ".html";
        await File.WriteAllTextAsync(tempHtml, @"
<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
</head>
<body>
    <h1>Vite + React</h1>
    <div id=""root"">
        <img alt=""React logo"" />
        <img alt=""Vite logo"" />
        <button>count is 0</button>
    </div>
</body>
</html>");

        try
        {
            // Create browser and page
            var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();

            // Navigate to the test HTML file
            await page.GotoAsync($"file://{tempHtml}");

            // Check that the page contains expected Vite React starter content
            var heading = await page.TextContentAsync("h1");
            Assert.Contains("Vite", heading);
            Assert.Contains("React", heading);

            // Check for the React logo
            var reactLogo = page.Locator("img[alt='React logo']");
            await Expect(reactLogo).ToBeVisibleAsync();

            // Check for the Vite logo  
            var viteLogo = page.Locator("img[alt='Vite logo']");
            await Expect(viteLogo).ToBeVisibleAsync();

            // Check for the counter button
            var button = page.Locator("button", new PageLocatorOptions { HasTextString = "count is" });
            await Expect(button).ToBeVisibleAsync();

            await browser.CloseAsync();
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempHtml))
            {
                File.Delete(tempHtml);
            }
        }
    }

    [Fact]
    public async Task Should_Demonstrate_Basic_Playwright_Functionality()
    {
        // Simple test to verify Playwright is working
        var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();
        
        // Navigate to about:blank and verify
        await page.GotoAsync("about:blank");
        
        // Set content and verify
        await page.SetContentAsync("<h1>Hello Playwright</h1>");
        var content = await page.TextContentAsync("h1");
        Assert.Equal("Hello Playwright", content);

        await browser.CloseAsync();
    }
}
