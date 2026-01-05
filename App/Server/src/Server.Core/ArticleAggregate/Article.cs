using Server.Core.AuthorAggregate;
using Server.Core.TagAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleAggregate;

public class Article : EntityBase, IAggregateRoot
{
  public const int TitleMaxLength = 200;
  public const int DescriptionMaxLength = 500;
  public const int SlugMaxLength = 250;

  public Article(string title, string description, string body, Author author)
  {
    Title = Guard.Against.NullOrEmpty(title);
    Description = Guard.Against.NullOrEmpty(description);
    Body = Guard.Against.NullOrEmpty(body);
    Author = Guard.Against.Null(author);
    AuthorId = author.Id;
    Slug = GenerateSlug(title);
    Tags = new List<Tag>();
    FavoritedBy = new List<Author>();
    Comments = new List<Comment>();
  }

  private Article()
  {
  } // For EF Core

  public string Title { get; private set; } = string.Empty;

  public string Description { get; private set; } = string.Empty;

  public string Body { get; private set; } = string.Empty;

  public string Slug { get; private set; } = string.Empty;

  public Guid AuthorId { get; private set; }

  public Author Author { get; private set; } = default!;

  public List<Tag> Tags { get; private set; } = new();

  public List<Author> FavoritedBy { get; private set; } = new();

  public List<Comment> Comments { get; private set; } = new();

  public int FavoritesCount => FavoritedBy.Count;

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

  /// <summary>
  /// Checks if the article is favorited by a specific user
  /// </summary>
  public bool IsFavoritedBy(Guid? userId)
  {
    if (!userId.HasValue)
    {
      return false;
    }

    return FavoritedBy.Any(a => a.Id == userId.Value);
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

  public void AddToFavorites(Author author)
  {
    if (!FavoritedBy.Contains(author))
    {
      FavoritedBy.Add(author);
    }
  }

  public void RemoveFromFavorites(Author author)
  {
    FavoritedBy.Remove(author);
  }
}
