using Server.Core.UserAggregate;

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
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    Tags = new List<Tag>();
    FavoritedBy = new List<User>();
  }

  private Article() { } // For EF Core

  public string Title { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public string Body { get; private set; } = string.Empty;
  public string Slug { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public int AuthorId { get; private set; }
  public User Author { get; private set; } = default!;

  public List<Tag> Tags { get; private set; } = new();
  public List<User> FavoritedBy { get; private set; } = new();

  public int FavoritesCount => FavoritedBy.Count;

  private static string GenerateSlug(string title)
  {
    return title.ToLowerInvariant()
      .Replace(" ", "-")
      .Replace(".", "")
      .Replace(",", "")
      .Replace("!", "")
      .Replace("?", "")
      .Replace("'", "")
      .Replace("\"", "");
  }

  public void Update(string title, string description, string body)
  {
    Title = Guard.Against.NullOrEmpty(title);
    Description = Guard.Against.NullOrEmpty(description);
    Body = Guard.Against.NullOrEmpty(body);
    Slug = GenerateSlug(title);
    UpdatedAt = DateTime.UtcNow;
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
