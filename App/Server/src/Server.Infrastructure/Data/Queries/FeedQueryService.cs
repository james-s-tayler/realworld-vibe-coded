using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;

namespace Server.Infrastructure.Data.Queries;

public class FeedQueryService(AppDbContext _context) : IFeedQueryService
{
  public async Task<IEnumerable<ArticleDto>> GetFeedAsync(
    int userId,
    int limit = 20,
    int offset = 0)
  {
    // Get IDs of users that the current user follows
    var followedUserIds = await _context.UserFollowings
      .AsNoTracking()
      .Where(uf => uf.FollowerId == userId)
      .Select(uf => uf.FollowedId)
      .ToListAsync();

    // If not following anyone, return empty list
    if (!followedUserIds.Any())
    {
      return new List<ArticleDto>();
    }

    // Direct LINQ projection to DTO - no entity materialization
    var articleDtos = await _context.Articles
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .OrderByDescending(a => a.CreatedAt)
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
        a.FavoritedBy.Any(u => u.Id == userId),
        a.FavoritedBy.Count,
        new AuthorDto(
          a.Author.Username,
          a.Author.Bio ?? string.Empty,
          a.Author.Image,
          true  // All authors in feed are followed by definition
        )
      ))
      .ToListAsync();

    return articleDtos;
  }

  public async Task<int> GetFeedCountAsync(int userId)
  {
    // Get IDs of users that the current user follows
    var followedUserIds = await _context.UserFollowings
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
    return await _context.Articles
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .CountAsync();
  }


}
