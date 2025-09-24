using Server.Core.ArticleAggregate.Dtos;

namespace Server.Core.Interfaces;

public interface IListArticlesQueryService
{
  Task<IEnumerable<ArticleDto>> ListAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0,
    int? currentUserId = null);

  Task<int> CountAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null);
}
