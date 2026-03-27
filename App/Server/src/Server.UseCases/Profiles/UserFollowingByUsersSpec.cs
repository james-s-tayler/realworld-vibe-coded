using Ardalis.Specification;
using Server.Core.UserFollowingAggregate;

namespace Server.UseCases.Profiles;

public class UserFollowingByUsersSpec : Specification<UserFollowing>
{
  public UserFollowingByUsersSpec(Guid followerId, Guid followedId)
  {
    Query.Where(x => x.FollowerId == followerId && x.FollowedId == followedId);
  }
}
