namespace Server.Core.AuthorAggregate.Specifications;

public class AuthorByUsernameSpec : Specification<Author>
{
  public AuthorByUsernameSpec(string username)
  {
    Query.Where(a => a.Username == username);
  }
}
