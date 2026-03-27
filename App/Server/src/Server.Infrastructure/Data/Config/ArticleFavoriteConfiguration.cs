using Server.Core.ArticleFavoriteAggregate;

namespace Server.Infrastructure.Data.Config;

public class ArticleFavoriteConfiguration : IEntityTypeConfiguration<ArticleFavorite>
{
  public void Configure(EntityTypeBuilder<ArticleFavorite> builder)
  {
    builder.HasIndex(x => new { x.UserId, x.ArticleId }).IsUnique();

    builder.HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.Article)
      .WithMany()
      .HasForeignKey(x => x.ArticleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
