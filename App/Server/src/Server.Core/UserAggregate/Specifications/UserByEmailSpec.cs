namespace Server.Core.UserAggregate.Specifications;

public class UserByEmailSpec : Specification<User>
{
  public UserByEmailSpec(string email)
  {
    Query.Where(user => user.Email == email);
  }
}
