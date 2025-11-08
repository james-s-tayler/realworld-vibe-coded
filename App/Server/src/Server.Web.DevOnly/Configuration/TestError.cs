namespace Server.Web.DevOnly.Configuration;

public sealed class TestError : SubGroup<DevOnly>
{
  public const string ROUTE = "test-error";

  public TestError()
  {
    Configure(ROUTE, ep =>
    {
      ep.AllowAnonymous();
    });
  }
}
