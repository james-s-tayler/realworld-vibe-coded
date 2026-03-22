namespace Server.FunctionalTests;

public class RegisteredUsers
{
  public IList<RegisteredUser> Users { get; } = new List<RegisteredUser>();

  public RegisteredUser GetOwner()
  {
    return Users.First();
  }
}
