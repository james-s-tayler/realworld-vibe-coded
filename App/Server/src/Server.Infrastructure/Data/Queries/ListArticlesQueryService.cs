using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;

namespace Server.Infrastructure.Data.Queries;

public class ListArticlesQueryService(AppDbContext _context) : IListArticlesQueryService
{
  public async Task<IEnumerable<ArticleDto>> ListAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0,
    int? currentUserId = null)
  {
    // Get list of user IDs that current user follows (for following status)
    List<int> followedUserIds = new();
    if (currentUserId.HasValue)
    {
      followedUserIds = await _context.UserFollowings
        .AsNoTracking()
        .Where(uf => uf.FollowerId == currentUserId.Value)
        .Select(uf => uf.FollowedId)
        .ToListAsync();
    }

    var query = BuildQuery(tag, author, favorited);

    // Direct LINQ projection to DTO - no entity materialization
    var articleDtos = await query
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .Select(a => new ArticleDto(
        a.Slug,
        a.Title,
        a.Description,
        a.Body,
        a.Tags.Select(t => t.Name).ToList(),
        a.CreatedAt,
        a.UpdatedAt,
        currentUserId.HasValue && a.FavoritedBy.Any(u => u.Id == currentUserId.Value),
        a.FavoritedBy.Count,
        new AuthorDto(
          a.Author.Username,
          a.Author.Bio ?? string.Empty,
          a.Author.Image,
          followedUserIds.Contains(a.AuthorId)
        )
      ))
      .ToListAsync();

    return articleDtos;
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
