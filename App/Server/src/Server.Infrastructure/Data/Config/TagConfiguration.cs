using Server.Core.TagAggregate;

namespace Server.Infrastructure.Data.Config;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
  public void Configure(EntityTypeBuilder<Tag> builder)
  {
    builder.Property(x => x.Name)
      .HasMaxLength(Tag.NameMaxLength)
      .IsRequired();

    builder.HasIndex(x => x.Name)
      .IsUnique();
  }
}
