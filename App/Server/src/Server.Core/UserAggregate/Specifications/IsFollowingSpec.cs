namespace Server.Core.UserAggregate.Specifications;

public class IsFollowingSpec : Specification<UserFollowing>
{
  public IsFollowingSpec(Guid followerId, Guid followedId)
  {
    Query.Where(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
  }
}
