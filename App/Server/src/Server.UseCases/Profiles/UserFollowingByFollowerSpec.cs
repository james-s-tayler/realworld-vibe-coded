using Ardalis.Specification;
using Server.Core.UserFollowingAggregate;

namespace Server.UseCases.Profiles;

public class UserFollowingByFollowerSpec : Specification<UserFollowing>
{
  public UserFollowingByFollowerSpec(Guid followerId)
  {
    Query.Where(x => x.FollowerId == followerId);
  }
}
