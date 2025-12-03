namespace E2eTests.Tests;

/// <summary>
/// Tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class SwaggerPageTests : ConduitPageTest
{
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
    finally
    {
      await SaveTrace("swagger_test");
    }
  }
}
