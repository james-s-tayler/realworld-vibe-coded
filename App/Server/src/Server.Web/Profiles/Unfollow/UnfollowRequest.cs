namespace Server.Web.Profiles.Unfollow;

public class UnfollowRequest
{
  [RouteParam]
  public string Username { get; set; } = string.Empty;
}
