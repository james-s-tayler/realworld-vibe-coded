namespace Server.Web.Users;

public class UserResponse
{
  public string Email { get; set; } = default!;
  public string Username { get; set; } = default!;
  public string Bio { get; set; } = default!;
  public string? Image { get; set; }
  public string Token { get; set; } = default!;
}
