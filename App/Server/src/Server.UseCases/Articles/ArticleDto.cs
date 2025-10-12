using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles;

public record ArticlesResponse(
  List<ArticleDto> Articles,
  int ArticlesCount
);

public record ArticlesEntitiesResult(
  List<Article> Articles,
  int ArticlesCount
);
