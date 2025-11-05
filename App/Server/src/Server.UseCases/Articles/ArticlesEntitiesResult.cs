using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles;

public record ArticlesEntitiesResult(
  List<Article> Articles,
  int ArticlesCount
);
