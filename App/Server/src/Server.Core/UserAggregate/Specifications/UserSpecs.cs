using Ardalis.Specification;

namespace Server.Core.UserAggregate;

public class UserByEmailSpec : Specification<User>
{
  public UserByEmailSpec(string email)
  {
    Query.Where(user => user.Email == email);
  }
}

public class UserByUsernameSpec : Specification<User>
{
  public UserByUsernameSpec(string username)
  {
    Query.Where(user => user.Username == username);
  }
}

public class UserByEmailAndPasswordSpec : Specification<User>
{
  public UserByEmailAndPasswordSpec(string email)
  {
    Query.Where(user => user.Email == email);
  }
}