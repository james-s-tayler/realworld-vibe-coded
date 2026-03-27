using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Config;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
  public void Configure(EntityTypeBuilder<Article> builder)
  {
    builder.Property(x => x.Slug)
      .HasMaxLength(Article.SlugMaxLength)
      .IsRequired();

    builder.Property(x => x.Title)
      .HasMaxLength(Article.TitleMaxLength)
      .IsRequired();

    builder.Property(x => x.Description)
      .HasMaxLength(Article.DescriptionMaxLength)
      .IsRequired();

    builder.Property(x => x.Body)
      .HasMaxLength(Article.BodyMaxLength)
      .IsRequired();

    builder.HasIndex(x => x.Slug).IsUnique();

    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(x => x.Tags)
      .WithMany(x => x.Articles)
      .UsingEntity("ArticleTag");
  }
}
