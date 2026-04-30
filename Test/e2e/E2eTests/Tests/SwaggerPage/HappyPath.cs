namespace E2eTests.Tests.SwaggerPage;

/// <summary>
/// Happy path tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  [TestCoverage(
    Id = "swagger-happy-001",
    FeatureArea = "swagger",
    Behavior = "Swagger API documentation page loads and displays API info",
    Verifies = ["API info section is visible"])]
  public async Task SwaggerApiDocs_AreDisplayed()
  {
    // Arrange & Act
    await Pages.SwaggerPage.GoToAsync();

    // Assert
    await Expect(Pages.SwaggerPage.ApiInfo).ToBeVisibleAsync();
  }
}
