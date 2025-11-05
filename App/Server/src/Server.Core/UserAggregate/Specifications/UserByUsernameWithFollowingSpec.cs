namespace Server.Core.UserAggregate.Specifications;

public class UserByUsernameWithFollowingSpec : Specification<User>
{
  public UserByUsernameWithFollowingSpec(string username)
  {
    Query.Where(u => u.Username == username)
         .Include(u => u.Following)
         .Include(u => u.Followers);
  }
}
