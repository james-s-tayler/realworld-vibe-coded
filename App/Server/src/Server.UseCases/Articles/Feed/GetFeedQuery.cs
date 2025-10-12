namespace Server.UseCases.Articles.Feed;

public record GetFeedQuery(
  int UserId,
  int Limit = 20,
  int Offset = 0
) : IQuery<Result<ArticlesEntitiesResult>>;
