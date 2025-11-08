using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Config;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
  public void Configure(EntityTypeBuilder<Tag> builder)
  {
    builder.Property(x => x.Name)
      .HasMaxLength(DataSchemaConstants.TagNameLength)
      .IsRequired();

    builder.HasIndex(x => x.Name)
      .IsUnique();
  }
}
