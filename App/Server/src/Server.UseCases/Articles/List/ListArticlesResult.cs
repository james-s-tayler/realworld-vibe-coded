using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.List;

public record ListArticlesResult(
  IEnumerable<Article> Articles,
  int TotalCount
);
