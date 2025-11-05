namespace Server.Core.UserAggregate.Specifications;

public class UserWithFollowingSpec : Specification<User>
{
  public UserWithFollowingSpec(Guid userId)
  {
    Query.Where(u => u.Id == userId)
         .Include(u => u.Following)
         .Include(u => u.Followers);
  }
}
