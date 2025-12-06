namespace E2eTests.Tests.SwaggerPage;
using static E2eTests.PageModels.Pages;

/// <summary>
/// Happy path tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Arrange & Act
    await Pages.SwaggerPage.GoToAsync();

    // Assert
    await Expect(Pages.SwaggerPage.ApiInfo).ToBeVisibleAsync();
  }
}
