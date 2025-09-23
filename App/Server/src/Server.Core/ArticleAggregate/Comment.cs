using Server.Core.UserAggregate;

namespace Server.Core.ArticleAggregate;

public class Comment : EntityBase
{
  public Comment(string body, User author, Article article)
  {
    Body = Guard.Against.NullOrEmpty(body);
    Author = Guard.Against.Null(author);
    AuthorId = author.Id;
    Article = Guard.Against.Null(article);
    ArticleId = article.Id;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  private Comment() { } // For EF Core

  public string Body { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public int AuthorId { get; private set; }
  public User Author { get; private set; } = default!;

  public int ArticleId { get; private set; }
  public Article Article { get; private set; } = default!;

  public void Update(string body)
  {
    Body = Guard.Against.NullOrEmpty(body);
    UpdatedAt = DateTime.UtcNow;
  }
}