using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Feed;

public record FeedArticlesQuery(
  Guid CurrentUserId,
  int Limit = 20,
  int Offset = 0) : IQuery<ArticlesListResult>;
