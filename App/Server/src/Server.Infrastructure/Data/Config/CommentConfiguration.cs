using Server.Core.CommentAggregate;

namespace Server.Infrastructure.Data.Config;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
  public void Configure(EntityTypeBuilder<Comment> builder)
  {
    builder.Property(x => x.Body)
      .HasMaxLength(Comment.BodyMaxLength)
      .IsRequired();

    builder.HasOne(x => x.Article)
      .WithMany()
      .HasForeignKey(x => x.ArticleId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
