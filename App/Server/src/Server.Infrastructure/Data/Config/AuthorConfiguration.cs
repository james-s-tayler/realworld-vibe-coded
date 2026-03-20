using Server.Core.AuthorAggregate;

namespace Server.Infrastructure.Data.Config;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
  public void Configure(EntityTypeBuilder<Author> builder)
  {
    builder.Property(x => x.Username)
      .HasMaxLength(Author.UsernameMaxLength)
      .IsRequired();

    builder.HasIndex(x => x.Username)
      .IsUnique();

    builder.Property(x => x.Bio)
      .HasMaxLength(Author.BioMaxLength)
      .IsRequired();

    builder.Property(x => x.Image)
      .HasMaxLength(Author.ImageUrlMaxLength);
  }
}
