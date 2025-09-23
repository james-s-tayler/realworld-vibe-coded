using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace Server.Core.ArticleAggregate;

public class Article : EntityBase, IAggregateRoot
{
  public Article(string slug, string title, string description, string body, int authorId)
  {
    Slug = Guard.Against.NullOrEmpty(slug, nameof(slug));
    Title = Guard.Against.NullOrEmpty(title, nameof(title));
    Description = Guard.Against.NullOrEmpty(description, nameof(description));
    Body = Guard.Against.NullOrEmpty(body, nameof(body));
    AuthorId = Guard.Against.NegativeOrZero(authorId, nameof(authorId));
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    FavoritesCount = 0;
    TagList = new List<string>();
  }

  public string Slug { get; private set; } = default!;
  public string Title { get; private set; } = default!;
  public string Description { get; private set; } = default!;
  public string Body { get; private set; } = default!;
  public int AuthorId { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public int FavoritesCount { get; private set; }
  public List<string> TagList { get; private set; } = new();

  public Article UpdateTitle(string newTitle)
  {
    Title = Guard.Against.NullOrEmpty(newTitle, nameof(newTitle));
    UpdatedAt = DateTime.UtcNow;
    return this;
  }

  public Article UpdateDescription(string newDescription)
  {
    Description = Guard.Against.NullOrEmpty(newDescription, nameof(newDescription));
    UpdatedAt = DateTime.UtcNow;
    return this;
  }

  public Article UpdateBody(string newBody)
  {
    Body = Guard.Against.NullOrEmpty(newBody, nameof(newBody));
    UpdatedAt = DateTime.UtcNow;
    return this;
  }

  public Article UpdateTags(List<string> newTags)
  {
    TagList = newTags ?? new List<string>();
    UpdatedAt = DateTime.UtcNow;
    return this;
  }

  public Article IncrementFavoritesCount()
  {
    FavoritesCount++;
    return this;
  }

  public Article DecrementFavoritesCount()
  {
    if (FavoritesCount > 0)
    {
      FavoritesCount--;
    }
    return this;
  }
}
