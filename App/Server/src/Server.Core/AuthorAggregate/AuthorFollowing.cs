using Server.SharedKernel.Persistence;

namespace Server.Core.AuthorAggregate;

public class AuthorFollowing : EntityBase
{
  public AuthorFollowing(Guid followerId, Guid followedId)
  {
    FollowerId = followerId;
    FollowedId = followedId;
  }

  // Required by EF Core
  private AuthorFollowing()
  {
  }

  public Guid FollowerId { get; private set; }

  public Guid FollowedId { get; private set; }

  // Navigation properties
  public Author Follower { get; private set; } = default!;

  public Author Followed { get; private set; } = default!;
}
