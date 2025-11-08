using Server.Core.UserAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleAggregate;

public class Article : EntityBase, IAggregateRoot
{
  public Article(string title, string description, string body, User author)
  {
    Title = Guard.Against.NullOrEmpty(title);
    Description = Guard.Against.NullOrEmpty(description);
    Body = Guard.Against.NullOrEmpty(body);
    Author = Guard.Against.Null(author);
    AuthorId = author.Id;
    Slug = GenerateSlug(title);
    Tags = new List<Tag>();
    FavoritedBy = new List<User>();
    Comments = new List<Comment>();
  }

  private Article() { } // For EF Core

  public string Title { get; private set; } = string.Empty;

  public string Description { get; private set; } = string.Empty;

  public string Body { get; private set; } = string.Empty;

  public string Slug { get; private set; } = string.Empty;

  public Guid AuthorId { get; private set; }

  public User Author { get; private set; } = default!;

  public List<Tag> Tags { get; private set; } = new();

  public List<User> FavoritedBy { get; private set; } = new();

  public List<Comment> Comments { get; private set; } = new();

  public int FavoritesCount => FavoritedBy.Count;

  /// <summary>
  /// Checks if the article is favorited by a specific user
  /// </summary>
  public bool IsFavoritedBy(Guid? userId)
  {
    if (!userId.HasValue)
    {
      return false;
    }
    return FavoritedBy.Any(u => u.Id == userId.Value);
  }

  /// <summary>
  /// Checks if a user is following the article's author
  /// </summary>
  public bool IsAuthorFollowedBy(User? user)
  {
    if (user == null)
    {
      return false;
    }
    return user.IsFollowing(AuthorId);
  }

  public static string GenerateSlug(string title)
  {
    return title.ToLowerInvariant()
      .Replace(" ", "-")
      .Replace(".", string.Empty)
      .Replace(",", string.Empty)
      .Replace("!", string.Empty)
      .Replace("?", string.Empty)
      .Replace("'", string.Empty)
      .Replace("\"", string.Empty);
  }

  public void Update(string title, string description, string body)
  {
    Title = Guard.Against.NullOrEmpty(title);
    Description = Guard.Against.NullOrEmpty(description);
    Body = Guard.Against.NullOrEmpty(body);
    Slug = GenerateSlug(title);
  }

  public void AddTag(Tag tag)
  {
    if (!Tags.Contains(tag))
    {
      Tags.Add(tag);
    }
  }

  public void RemoveTag(Tag tag)
  {
    Tags.Remove(tag);
  }

  public void AddToFavorites(User user)
  {
    if (!FavoritedBy.Contains(user))
    {
      FavoritedBy.Add(user);
    }
  }

  public void RemoveFromFavorites(User user)
  {
    FavoritedBy.Remove(user);
  }
}
