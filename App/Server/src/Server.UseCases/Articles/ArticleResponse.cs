using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles;

public class ArticleResponse
{
  public ArticleDto Article { get; set; } = default!;
}
