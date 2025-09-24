using Server.Core.ArticleAggregate.Dtos;

namespace Server.Core.Interfaces;

public interface IFeedQueryService
{
  Task<IEnumerable<ArticleDto>> GetFeedAsync(
    int userId,
    int limit = 20,
    int offset = 0);

  Task<int> GetFeedCountAsync(int userId);
}
