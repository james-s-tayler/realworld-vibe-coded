namespace Server.Web.Profiles;

public class ProfileDto
{
  public string Username { get; set; } = string.Empty;

  public string Bio { get; set; } = string.Empty;

  public string? Image { get; set; }

  public bool Following { get; set; }
}
