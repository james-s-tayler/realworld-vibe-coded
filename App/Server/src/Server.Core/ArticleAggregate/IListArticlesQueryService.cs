namespace Server.Core.ArticleAggregate;

public interface IListArticlesQueryService
{
  Task<IEnumerable<Article>> ListAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0,
    Guid? currentUserId = null);
}
