using Server.Core.ArticleAggregate;

namespace Server.Core.Interfaces;

public interface IFeedQueryService
{
  Task<IEnumerable<Article>> GetFeedAsync(
    int userId,
    int limit = 20,
    int offset = 0);

  Task<int> GetFeedCountAsync(int userId);
}
