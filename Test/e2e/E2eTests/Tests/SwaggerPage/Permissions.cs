namespace E2eTests.Tests.SwaggerPage;

/// <summary>
/// Permission tests for the Swagger API documentation page (/swagger/index.html).
/// </summary>
[Collection("E2E Tests")]
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  // No permission tests for SwaggerPage currently
}
