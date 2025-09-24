using Ardalis.Specification;
using Server.Core.UserAggregate;

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

public class UserWithFollowingSpec : Specification<User>
{
  public UserWithFollowingSpec(int userId)
  {
    Query.Where(u => u.Id == userId)
         .Include(u => u.Following)
         .Include(u => u.Followers);
  }
}

public class IsFollowingSpec : Specification<UserFollowing>
{
  public IsFollowingSpec(int followerId, int followedId)
  {
    Query.Where(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
  }
}
