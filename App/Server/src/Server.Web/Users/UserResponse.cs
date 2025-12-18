namespace Server.Web.Users;

public class UserResponse
{
  public string EmailAddress { get; set; } = default!;

  public string Username { get; set; } = default!;

  public string Bio { get; set; } = default!;

  public string? Image { get; set; }

  public string Token { get; set; } = default!;
}
