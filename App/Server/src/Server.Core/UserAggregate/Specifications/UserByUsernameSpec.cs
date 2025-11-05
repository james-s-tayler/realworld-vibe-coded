namespace Server.Core.UserAggregate.Specifications;

public class UserByUsernameSpec : Specification<User>
{
  public UserByUsernameSpec(string username)
  {
    Query.Where(user => user.Username == username);
  }
}
