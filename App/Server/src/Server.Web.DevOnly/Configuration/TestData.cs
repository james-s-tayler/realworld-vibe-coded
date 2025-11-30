namespace Server.Web.DevOnly.Configuration;

/// <summary>
/// Group for test data management endpoints.
/// </summary>
public sealed class TestData : SubGroup<DevOnly>
{
  public const string ROUTE = "test-data";

  public TestData()
  {
    Configure(ROUTE, ep =>
    {
      ep.AllowAnonymous();
    });
  }
}
