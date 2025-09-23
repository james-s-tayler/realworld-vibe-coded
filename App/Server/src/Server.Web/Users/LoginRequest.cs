using System.ComponentModel.DataAnnotations;

namespace Server.Web.Users;

public class LoginRequest
{
  public const string Route = "/api/users/login";

  public LoginUserData User { get; set; } = new();
}

public class LoginUserData
{
  [Required]
  public string Email { get; set; } = default!;
  
  [Required]
  public string Password { get; set; } = default!;
}

public class LoginResponse
{
  public UserResponse User { get; set; } = default!;
}