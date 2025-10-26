namespace Server.Core.UserAggregate.Specifications;

public class UserByUsernameWithFollowingSpec : Specification<User>
{
  public UserByUsernameWithFollowingSpec(string username)
  {
    var normalizedUsername = username.ToUpperInvariant();
    Query.Where(u => u.NormalizedUserName == normalizedUsername)
         .Include(u => u.Following)
         .Include(u => u.Followers);
  }
}

public class UserWithFollowingSpec : Specification<User>
{
  public UserWithFollowingSpec(Guid userId)
  {
    Query.Where(u => u.Id == userId)
         .Include(u => u.Following)
         .Include(u => u.Followers);
  }
}

public class IsFollowingSpec : Specification<UserFollowing>
{
  public IsFollowingSpec(Guid followerId, Guid followedId)
  {
    Query.Where(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
  }
}
