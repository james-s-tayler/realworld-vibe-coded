namespace Server.Web.Users.Login;

public class LoginRequest
{
  public const string Route = "/api/users/login";

  public LoginUserData User { get; set; } = new();
}
