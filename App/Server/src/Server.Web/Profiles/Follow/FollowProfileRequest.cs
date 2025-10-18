namespace Server.Web.Profiles.Follow;

public class FollowProfileRequest
{
  [RouteParam]
  public string Username { get; set; } = string.Empty;
}
