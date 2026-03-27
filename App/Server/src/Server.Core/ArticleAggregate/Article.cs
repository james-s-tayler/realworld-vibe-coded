using Server.Core.IdentityAggregate;
using Server.Core.TagAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleAggregate;

public class Article : EntityBase, IAggregateRoot
{
  public const int TitleMaxLength = 255;
  public const int DescriptionMaxLength = 1000;
  public const int BodyMaxLength = 50000;
  public const int SlugMaxLength = 300;

  public string Slug { get; set; } = default!;

  public string Title { get; set; } = default!;

  public string Description { get; set; } = default!;

  public string Body { get; set; } = default!;

  public Guid AuthorId { get; set; }

  public ApplicationUser Author { get; set; } = default!;

  public List<string> TagList { get; set; } = [];

  public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
