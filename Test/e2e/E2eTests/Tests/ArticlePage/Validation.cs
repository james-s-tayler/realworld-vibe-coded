namespace E2eTests.Tests.ArticlePage;

/// <summary>
/// Validation tests for the Article page (/article/:slug).
/// </summary>
[Collection("E2E Tests")]
public class Validation : AppPageTest
{
  public Validation(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  // No validation tests for ArticlePage currently
}
