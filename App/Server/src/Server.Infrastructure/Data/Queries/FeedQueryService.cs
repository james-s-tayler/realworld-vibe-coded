using Server.Core.ArticleAggregate;

namespace Server.Infrastructure.Data.Queries;

public class FeedQueryService(AppDbContext context) : IFeedQueryService
{
  public async Task<IEnumerable<Article>> GetFeedAsync(
    Guid userId,
    int limit = 20,
    int offset = 0)
  {
    // Get IDs of users that the current user follows
    var followedUserIds = await context.UserFollowings
      .AsNoTracking()
      .Where(uf => uf.FollowerId == userId)
      .Select(uf => uf.FollowedId)
      .ToListAsync();

    // If not following anyone, return empty list
    if (!followedUserIds.Any())
    {
      return new List<Article>();
    }

    // Get articles from followed users
    var articles = await context.Articles
      .Include(a => a.Author)
      .Include(a => a.Tags)
      .Include(a => a.FavoritedBy)
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .OrderByDescending(a => a.CreatedAt)
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .ToListAsync();

    return articles;
  }

  public async Task<int> GetFeedCountAsync(Guid userId)
  {
    // Get IDs of users that the current user follows
    var followedUserIds = await context.UserFollowings
      .AsNoTracking()
      .Where(uf => uf.FollowerId == userId)
      .Select(uf => uf.FollowedId)
      .ToListAsync();

    // If not following anyone, count is 0
    if (!followedUserIds.Any())
    {
      return 0;
    }

    // Count articles from followed users
    return await context.Articles
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .CountAsync();
  }
}
