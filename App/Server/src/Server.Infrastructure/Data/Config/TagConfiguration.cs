using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.TagAggregate;

namespace Server.Infrastructure.Data.Config;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
  public void Configure(EntityTypeBuilder<Tag> builder)
  {
    builder.Property(p => p.Name)
        .HasMaxLength(DataSchemaConstants.TAG_NAME_LENGTH)
        .IsRequired();

    // Create unique index on tag name
    builder.HasIndex(p => p.Name)
        .IsUnique()
        .HasDatabaseName("IX_Tags_Name");
  }
}
