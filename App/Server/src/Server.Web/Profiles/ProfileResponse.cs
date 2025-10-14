namespace Server.Web.Profiles;

public class ProfileResponse
{
  public ProfileDto Profile { get; set; } = default!;
}

public class ProfileDto
{
  public string Username { get; set; } = string.Empty;
  public string Bio { get; set; } = string.Empty;
  public string? Image { get; set; }
  public bool Following { get; set; }
}
