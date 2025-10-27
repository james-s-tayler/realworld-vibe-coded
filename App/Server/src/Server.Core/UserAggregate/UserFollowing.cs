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
  private UserFollowing() { }

  public Guid FollowerId { get; private set; }
  public Guid FollowedId { get; private set; }

  // Navigation properties
  public User Follower { get; private set; } = default!;
  public User Followed { get; private set; } = default!;
}
