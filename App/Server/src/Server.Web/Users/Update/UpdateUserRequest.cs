namespace Server.Web.Users.Update;

public class UpdateUserRequest
{
  public const string Route = "/api/user";

  public UpdateUserData User { get; set; } = new();
}
