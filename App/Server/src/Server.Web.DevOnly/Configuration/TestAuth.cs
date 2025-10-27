namespace Server.Web.DevOnly.Configuration;

public class TestAuth : SubGroup<DevOnly>
{
  public const string ROUTE = "test-auth";

  public TestAuth()
  {
    Configure("test-auth", ep => { });
  }
}
