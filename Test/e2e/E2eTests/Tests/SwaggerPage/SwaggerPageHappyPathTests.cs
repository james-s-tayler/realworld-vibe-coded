namespace E2eTests.Tests.SwaggerPage;

/// <summary>
/// Happy path tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class SwaggerPageHappyPathTests : AppPageTest
{
  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Navigate to Swagger docs using page model
    var swaggerPage = GetSwaggerPage();
    await swaggerPage.GoToAsync();

    await Expect(swaggerPage.ApiInfo).ToBeVisibleAsync();
  }
}
