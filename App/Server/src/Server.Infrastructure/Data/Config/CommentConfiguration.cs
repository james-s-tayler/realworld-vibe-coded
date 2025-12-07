using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Config;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
  public void Configure(EntityTypeBuilder<Comment> builder)
  {
    builder.Property(x => x.Body)
      .HasMaxLength(Comment.BodyMaxLength)
      .IsRequired();

    // One-to-many relationship with User (Author)
    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);

    // One-to-many relationship with Article
    builder.HasOne(x => x.Article)
      .WithMany(x => x.Comments)
      .HasForeignKey(x => x.ArticleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
