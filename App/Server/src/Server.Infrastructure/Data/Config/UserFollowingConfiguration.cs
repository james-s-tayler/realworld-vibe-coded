using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.UserAggregate;

namespace Server.Infrastructure.Data.Config;

public class UserFollowingConfiguration : IEntityTypeConfiguration<UserFollowing>
{
  public void Configure(EntityTypeBuilder<UserFollowing> builder)
  {
    builder.HasKey(uf => new { uf.FollowerId, uf.FollowedId });

    builder.Property(uf => uf.FollowerId)
      .IsRequired();

    builder.Property(uf => uf.FollowedId)
      .IsRequired();

    // Configure relationships
    builder.HasOne(uf => uf.Follower)
      .WithMany(u => u.Following)
      .HasForeignKey(uf => uf.FollowerId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(uf => uf.Followed)
      .WithMany(u => u.Followers)
      .HasForeignKey(uf => uf.FollowedId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.ToTable("UserFollowing");
  }
}
