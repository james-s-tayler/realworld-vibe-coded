namespace Server.Web.Users.Deactivate;

public class DeactivateUserRequest
{
  [RouteParam]
  public Guid UserId { get; set; }
}
