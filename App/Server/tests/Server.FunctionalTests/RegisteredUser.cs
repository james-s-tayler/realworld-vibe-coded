namespace Server.FunctionalTests;

public class RegisteredUser
{
  public required string Email { get; set; }

  public required HttpClient Client { get; set; }
}
