namespace Server.Web.Users.Register;

public class RegisterRequest
{
  public const string Route = "/api/users";

  public UserData User { get; set; } = new();
}

public class UserData
{
  public string Email { get; set; } = default!;
  public string Username { get; set; } = default!;
  public string Password { get; set; } = default!;
}

public class RegisterResponse
{
  public UserResponse User { get; set; } = default!;
}

public class UserResponse
{
  public string Email { get; set; } = default!;
  public string Username { get; set; } = default!;
  public string Bio { get; set; } = default!;
  public string? Image { get; set; }
  public string Token { get; set; } = default!;
}
