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
    Following = new List<AuthorFollowing>();
    Followers = new List<AuthorFollowing>();
  }

  private Author()
  {
    Following = new List<AuthorFollowing>();
    Followers = new List<AuthorFollowing>();
  }

  public string Username { get; private set; } = string.Empty;

  public string Bio { get; private set; } = string.Empty;

  public string? Image { get; private set; }

  // Navigation properties for following relationships
  public ICollection<AuthorFollowing> Following { get; private set; } = new List<AuthorFollowing>();

  public ICollection<AuthorFollowing> Followers { get; private set; } = new List<AuthorFollowing>();

  public void Update(string username, string bio, string? image)
  {
    Username = Guard.Against.NullOrEmpty(username);
    Bio = bio ?? string.Empty;
    Image = image;
  }

  /// <summary>
  /// Follow another author
  /// </summary>
  public void Follow(Author authorToFollow)
  {
    if (authorToFollow.Id == Id)
    {
      return; // Cannot follow yourself
    }

    if (Following.Any(f => f.FollowedId == authorToFollow.Id))
    {
      return; // Already following
    }

    var following = new AuthorFollowing(Id, authorToFollow.Id);
    Following.Add(following);
  }

  /// <summary>
  /// Unfollow an author
  /// </summary>
  public void Unfollow(Author authorToUnfollow)
  {
    var following = Following.FirstOrDefault(f => f.FollowedId == authorToUnfollow.Id);
    if (following != null)
    {
      Following.Remove(following);
    }
  }

  /// <summary>
  /// Check if this author is following another author
  /// </summary>
  public bool IsFollowing(Author author)
  {
    return Following.Any(f => f.FollowedId == author.Id);
  }

  /// <summary>
  /// Check if this author is following an author by ID
  /// </summary>
  public bool IsFollowing(Guid authorId)
  {
    return Following.Any(f => f.FollowedId == authorId);
  }
}
