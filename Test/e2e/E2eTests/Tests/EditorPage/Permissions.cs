namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Permission tests for the Editor page (/editor and /editor/:slug).
/// </summary>
public class Permissions : AppPageTest
{
  public Permissions(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  // No permission tests for EditorPage currently - protected route tests are in LoginPage
}
