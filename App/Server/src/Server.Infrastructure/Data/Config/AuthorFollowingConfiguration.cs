using Server.Core.AuthorAggregate;

namespace Server.Infrastructure.Data.Config;

public class AuthorFollowingConfiguration : IEntityTypeConfiguration<AuthorFollowing>
{
  public void Configure(EntityTypeBuilder<AuthorFollowing> builder)
  {
    builder.ToTable("AuthorFollowing");

    builder.HasKey(x => x.Id);

    // Follower relationship
    builder.HasOne(x => x.Follower)
      .WithMany(x => x.Following)
      .HasForeignKey(x => x.FollowerId)
      .OnDelete(DeleteBehavior.Restrict);

    // Followed relationship
    builder.HasOne(x => x.Followed)
      .WithMany(x => x.Followers)
      .HasForeignKey(x => x.FollowedId)
      .OnDelete(DeleteBehavior.Restrict);

    // Unique constraint to prevent duplicate following relationships
    builder.HasIndex(x => new { x.FollowerId, x.FollowedId })
      .IsUnique();
  }
}
