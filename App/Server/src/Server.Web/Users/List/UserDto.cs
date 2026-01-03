namespace Server.Web.Users.List;

public class UserDto
{
  public required string Email { get; set; }

  public required string Username { get; set; }

  public string Bio { get; set; } = string.Empty;

  public string? Image { get; set; }
}
