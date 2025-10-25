namespace Server.Core.UserAggregate;

public class UserByEmailSpec : Specification<User>
{
  public UserByEmailSpec(string email)
  {
    Query.Where(user => user.NormalizedEmail == email.ToUpperInvariant());
  }
}

public class UserByUsernameSpec : Specification<User>
{
  public UserByUsernameSpec(string username)
  {
    Query.Where(user => user.NormalizedUserName == username.ToUpperInvariant());
  }
}

public class UserByEmailAndPasswordSpec : Specification<User>
{
  public UserByEmailAndPasswordSpec(string email)
  {
    Query.Where(user => user.NormalizedEmail == email.ToUpperInvariant());
  }
}
