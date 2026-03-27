using Server.Core.IdentityAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.UserFollowingAggregate;

public class UserFollowing : EntityBase, IAggregateRoot
{
  public Guid FollowerId { get; set; }

  public ApplicationUser Follower { get; set; } = default!;

  public Guid FollowedId { get; set; }

  public ApplicationUser Followed { get; set; } = default!;
}
