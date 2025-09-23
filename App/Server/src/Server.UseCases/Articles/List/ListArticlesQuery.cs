using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Server.UseCases.Articles.List;

public record ListArticlesQuery(
  string? Tag = null,
  string? Author = null,
  string? Favorited = null,
  int Limit = 20,
  int Offset = 0
) : IQuery<Result<ListArticlesResult>>;

public class ListArticlesResult
{
  public List<ArticleDto> Articles { get; set; } = new();
  public int ArticlesCount { get; set; }
}
