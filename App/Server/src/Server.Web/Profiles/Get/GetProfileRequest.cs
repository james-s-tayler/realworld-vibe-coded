namespace Server.Web.Profiles.Get;

public class GetProfileRequest
{
  [RouteParam]
  public string Username { get; set; } = string.Empty;
}
