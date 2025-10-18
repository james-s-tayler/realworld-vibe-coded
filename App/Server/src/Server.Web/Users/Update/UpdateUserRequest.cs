using Server.Web.Users.Register;

namespace Server.Web.Users.Update;

public class UpdateUserRequest
{
  public const string Route = "/api/user";

  public UpdateUserData User { get; set; } = new();
}

public class UpdateUserData
{
  public string? Email { get; set; }
  public string? Username { get; set; }
  public string? Password { get; set; }
  public string? Bio { get; set; }
  public string? Image { get; set; }
}

public class UpdateUserResponse
{
  public UserResponse User { get; set; } = default!;
}
