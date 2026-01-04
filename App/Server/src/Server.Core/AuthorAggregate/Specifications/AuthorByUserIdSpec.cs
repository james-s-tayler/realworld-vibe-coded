namespace Server.Core.AuthorAggregate.Specifications;

public class AuthorByUserIdSpec : Specification<Author>
{
  public AuthorByUserIdSpec(Guid userId)
  {
    Query.Where(x => x.Id == userId);
  }
}
