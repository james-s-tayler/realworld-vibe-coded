namespace Server.FunctionalTests;

public class RegisteredTenant
{
  public IList<RegisteredUser> Users { get; } = new List<RegisteredUser>();

  public RegisteredUser GetTenantOwner()
  {
    return Users.First();
  }
}
