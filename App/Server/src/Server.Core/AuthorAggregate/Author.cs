using Server.SharedKernel.Persistence;

namespace Server.Core.AuthorAggregate;

public class Author : EntityBase, IAggregateRoot
{
  public const int UsernameMinLength = 2;
  public const int UsernameMaxLength = 100;
  public const int BioMaxLength = 1000;
  public const int ImageUrlMaxLength = 500;

  public Author(Guid id, string username, string bio, string? image)
  {
    Id = id;
    Username = Guard.Against.NullOrEmpty(username);
    Bio = bio ?? string.Empty;
    Image = image;
  }

  private Author()
  {
  }

  public string Username { get; private set; } = string.Empty;

  public string Bio { get; private set; } = string.Empty;

  public string? Image { get; private set; }

  public void Update(string username, string bio, string? image)
  {
    Username = Guard.Against.NullOrEmpty(username);
    Bio = bio ?? string.Empty;
    Image = image;
  }
}
