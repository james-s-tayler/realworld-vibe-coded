namespace Server.Web.DevOnly.Configuration;

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
