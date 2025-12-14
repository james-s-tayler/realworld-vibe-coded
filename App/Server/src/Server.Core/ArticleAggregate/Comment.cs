using Server.Core.IdentityAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleAggregate;

public class Comment : EntityBase
{
  public const int BodyMaxLength = 5000;

  public Comment(string body, ApplicationUser author, Article article)
  {
    Body = Guard.Against.NullOrEmpty(body);
    Author = Guard.Against.Null(author);
    AuthorId = author.Id;
    Article = Guard.Against.Null(article);
    ArticleId = article.Id;
  }

  private Comment()
  {
  } // For EF Core

  public string Body { get; private set; } = string.Empty;

  public Guid AuthorId { get; private set; }

  public ApplicationUser Author { get; private set; } = default!;

  public Guid ArticleId { get; private set; }

  public Article Article { get; private set; } = default!;

  public void Update(string body)
  {
    Body = Guard.Against.NullOrEmpty(body);
  }
}
