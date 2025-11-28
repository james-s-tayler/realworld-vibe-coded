using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Feed;

public record GetFeedResult(
  IEnumerable<Article> Articles,
  int TotalCount
);
