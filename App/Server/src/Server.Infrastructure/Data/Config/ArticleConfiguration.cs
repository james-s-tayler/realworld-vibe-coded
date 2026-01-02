using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Config;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
  public void Configure(EntityTypeBuilder<Article> builder)
  {
    builder.Property(x => x.Title)
      .HasMaxLength(Article.TitleMaxLength)
      .IsRequired();

    builder.Property(x => x.Description)
      .HasMaxLength(Article.DescriptionMaxLength)
      .IsRequired();

    builder.Property(x => x.Body)
      .IsRequired();

    builder.Property(x => x.Slug)
      .HasMaxLength(Article.SlugMaxLength)
      .IsRequired();

    builder.HasIndex(x => x.Slug)
      .IsUnique();

    // One-to-many relationship with User (Author)
    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);

    // Many-to-many relationship with Tags
    builder.HasMany(x => x.Tags)
      .WithMany(x => x.Articles)
      .UsingEntity(j => j.ToTable("ArticleTags"));

    // Many-to-many relationship with Users (Favorites)
    builder.HasMany(x => x.FavoritedBy)
      .WithMany()
      .UsingEntity(j => j.ToTable("ArticleFavorites"));
  }
}
