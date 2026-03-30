namespace Server.Web.Users.Reactivate;

public class ReactivateUserRequest
{
  [RouteParam]
  public Guid UserId { get; set; }
}
