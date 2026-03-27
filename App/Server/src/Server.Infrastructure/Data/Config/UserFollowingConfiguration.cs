using Server.Core.UserFollowingAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserFollowingConfiguration : IEntityTypeConfiguration<UserFollowing>
{
  public void Configure(EntityTypeBuilder<UserFollowing> builder)
  {
    builder.HasIndex(x => new { x.FollowerId, x.FollowedId }).IsUnique();

    builder.HasOne(x => x.Follower)
      .WithMany()
      .HasForeignKey(x => x.FollowerId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.Followed)
      .WithMany()
      .HasForeignKey(x => x.FollowedId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
