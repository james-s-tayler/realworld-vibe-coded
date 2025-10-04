namespace Server.Core.UserAggregate;

public class UserFollowing : EntityBase
{
  public UserFollowing(int followerId, int followedId)
  {
    FollowerId = followerId;
    FollowedId = followedId;
  }

  // Required by EF Core
  private UserFollowing() { }

  public int FollowerId { get; private set; }
  public int FollowedId { get; private set; }

  // Navigation properties
  public User Follower { get; private set; } = default!;
  public User Followed { get; private set; } = default!;
}
