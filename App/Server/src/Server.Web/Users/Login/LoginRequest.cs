using Server.Web.Users.Register;

namespace Server.Web.Users.Login;

public class LoginRequest
{
  public const string Route = "/api/users/login";

  public LoginUserData User { get; set; } = new();
}

public class LoginUserData
{
  public string Email { get; set; } = default!;
  public string Password { get; set; } = default!;
}

public class LoginResponse
{
  public UserResponse User { get; set; } = default!;
}
