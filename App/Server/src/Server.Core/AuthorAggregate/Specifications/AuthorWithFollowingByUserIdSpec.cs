namespace Server.Core.AuthorAggregate.Specifications;

public class AuthorWithFollowingByUserIdSpec : Specification<Author>
{
  public AuthorWithFollowingByUserIdSpec(Guid userId)
  {
    Query.Where(a => a.Id == userId)
         .Include(a => a.Following);
  }
}
