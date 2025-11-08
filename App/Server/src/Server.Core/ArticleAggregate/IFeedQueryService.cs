namespace Server.Core.ArticleAggregate;

public interface IFeedQueryService
{
  Task<IEnumerable<Article>> GetFeedAsync(
    Guid userId,
    int limit = 20,
    int offset = 0);
}
