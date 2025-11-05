namespace Server.Web.Users.Register;

public class RegisterRequest
{
  public const string Route = "/api/users";

  public UserData User { get; set; } = new();
}
