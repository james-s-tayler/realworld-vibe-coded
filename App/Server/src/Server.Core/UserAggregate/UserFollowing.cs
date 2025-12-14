using Server.Core.IdentityAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.UserAggregate;

public class UserFollowing : EntityBase
{
  public UserFollowing(Guid followerId, Guid followedId)
  {
    FollowerId = followerId;
    FollowedId = followedId;
  }

  // Required by EF Core
  private UserFollowing()
  {
  }

  public Guid FollowerId { get; private set; }

  public Guid FollowedId { get; private set; }

  // Navigation properties
  public ApplicationUser Follower { get; private set; } = default!;

  public ApplicationUser Followed { get; private set; } = default!;
}
