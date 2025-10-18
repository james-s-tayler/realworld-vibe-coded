namespace Server.Web.Profiles.Unfollow;

public class UnfollowProfileRequest
{
  [RouteParam]
  public string Username { get; set; } = string.Empty;
}
