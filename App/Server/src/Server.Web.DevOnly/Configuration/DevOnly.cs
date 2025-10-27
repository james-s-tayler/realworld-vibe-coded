namespace Server.Web.DevOnly.Configuration;

public sealed class DevOnly : Group
{
  public const string ROUTE = "api/dev-only";
  public DevOnly()
  {
    Configure(ROUTE, ep => { });
  }
}
