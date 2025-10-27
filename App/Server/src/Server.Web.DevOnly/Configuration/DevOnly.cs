namespace Server.Web.DevOnly.Configuration;

public sealed class DevOnly : Group
{
  public const string ROUTE = "dev-only";
  public DevOnly()
  {
    Configure(ROUTE, ep => { });
  }
}
