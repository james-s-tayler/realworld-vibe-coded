namespace E2eTests.Tests.SwaggerPage;

/// <summary>
/// Happy path tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class SwaggerPageHappyPathTests : ConduitPageTest
{
  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Navigate to Swagger docs using page model
    var swaggerPage = GetSwaggerPage();
    await swaggerPage.GoToAsync();

    // Wait for Swagger UI to load
    await swaggerPage.WaitForSwaggerToLoadAsync();

    var isVisible = await swaggerPage.IsSwaggerVisibleAsync();
    Assert.True(isVisible, "Swagger UI container should be visible");

    // Verify that the API title is displayed
    var title = await swaggerPage.GetApiTitleAsync();
    Assert.NotNull(title);
    Assert.NotEmpty(title.Trim());
  }
}
