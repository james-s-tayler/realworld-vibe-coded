namespace E2eTests.Tests.SwaggerPage;

/// <summary>
/// Happy path tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class HappyPath : AppPageTest
{
  [Fact]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Arrange
    var swaggerPage = GetSwaggerPage();

    // Act
    await swaggerPage.GoToAsync();

    // Assert
    await Expect(swaggerPage.ApiInfo).ToBeVisibleAsync();
  }
}
