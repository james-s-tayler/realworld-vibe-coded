using Server.Core.ArticleAggregate;
using Server.Core.Interfaces;

namespace Server.Infrastructure.Data.Queries;

public class ListArticlesQueryService(AppDbContext _context) : IListArticlesQueryService
{
  public async Task<IEnumerable<Article>> ListAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0,
    Guid? currentUserId = null)
  {
    var query = BuildQuery(tag, author, favorited);

    var articles = await query
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .ToListAsync();

    return articles;
  }

  public async Task<int> CountAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null)
  {
    var query = BuildQuery(tag, author, favorited);
    return await query.CountAsync();
  }

  private IQueryable<Article> BuildQuery(string? tag, string? author, string? favorited)
  {
    var query = _context.Articles
      .Include(a => a.Author)
      .Include(a => a.Tags)
      .Include(a => a.FavoritedBy)
      .AsQueryable();

    if (!string.IsNullOrEmpty(tag))
    {
      query = query.Where(a => a.Tags.Any(t => t.Name == tag));
    }

    if (!string.IsNullOrEmpty(author))
    {
      query = query.Where(a => a.Author.Username == author);
    }

    if (!string.IsNullOrEmpty(favorited))
    {
      query = query.Where(a => a.FavoritedBy.Any(u => u.Username == favorited));
    }

    return query.OrderByDescending(a => a.CreatedAt);
  }
}
