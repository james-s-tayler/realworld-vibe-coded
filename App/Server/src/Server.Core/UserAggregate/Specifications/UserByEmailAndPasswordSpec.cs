namespace Server.Core.UserAggregate.Specifications;

public class UserByEmailAndPasswordSpec : Specification<User>
{
  public UserByEmailAndPasswordSpec(string email)
  {
    Query.Where(user => user.Email == email);
  }
}
