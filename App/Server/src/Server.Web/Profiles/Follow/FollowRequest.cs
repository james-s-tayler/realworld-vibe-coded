namespace Server.Web.Profiles.Follow;

public class FollowRequest
{
  [RouteParam]
  public string Username { get; set; } = string.Empty;
}
