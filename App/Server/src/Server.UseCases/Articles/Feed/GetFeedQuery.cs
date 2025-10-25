using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Feed;

public record GetFeedQuery(
  Guid UserId,
  int Limit = 20,
  int Offset = 0
) : IQuery<IEnumerable<Article>>;
