using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Config;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
  public void Configure(EntityTypeBuilder<Article> builder)
  {
    builder.Property(p => p.Slug)
        .HasMaxLength(DataSchemaConstants.ARTICLE_SLUG_LENGTH)
        .IsRequired();

    builder.Property(p => p.Title)
        .HasMaxLength(DataSchemaConstants.ARTICLE_TITLE_LENGTH)
        .IsRequired();

    builder.Property(p => p.Description)
        .HasMaxLength(DataSchemaConstants.ARTICLE_DESCRIPTION_LENGTH)
        .IsRequired();

    builder.Property(p => p.Body)
        .HasMaxLength(DataSchemaConstants.ARTICLE_BODY_LENGTH)
        .IsRequired();

    builder.Property(p => p.AuthorId)
        .IsRequired();

    builder.Property(p => p.CreatedAt)
        .IsRequired();

    builder.Property(p => p.UpdatedAt)
        .IsRequired();

    builder.Property(p => p.FavoritesCount)
        .IsRequired();

    // Store tag list as JSON
    builder.Property(p => p.TagList)
        .HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
        );

    // Create unique index on slug
    builder.HasIndex(p => p.Slug)
        .IsUnique()
        .HasDatabaseName("IX_Articles_Slug");

    // Create index on AuthorId for faster queries
    builder.HasIndex(p => p.AuthorId)
        .HasDatabaseName("IX_Articles_AuthorId");

    // Create index on CreatedAt for ordering
    builder.HasIndex(p => p.CreatedAt)
        .HasDatabaseName("IX_Articles_CreatedAt");
  }
}
