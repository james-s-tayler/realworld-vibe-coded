namespace Server.Core.AuthorAggregate.Specifications;

public class AuthorWithRelationshipsByUsernameSpec : Specification<Author>
{
  public AuthorWithRelationshipsByUsernameSpec(string username)
  {
    Query.Where(a => a.Username == username)
         .Include(a => a.Following)
         .Include(a => a.Followers);
  }
}
