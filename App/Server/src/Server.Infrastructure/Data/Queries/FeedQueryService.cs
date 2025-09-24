using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;

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

    // Get articles from followed users
    var articles = await _context.Articles
      .Include(a => a.Author)
      .Include(a => a.Tags)
      .Include(a => a.FavoritedBy)
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .OrderByDescending(a => a.CreatedAt)
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .ToListAsync();

    // Get current user with following relationships
    var currentUser = await _context.Users
      .Include(u => u.Following)
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == userId);

    return articles.Select(a => MapToDto(a, currentUser));
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

  private static ArticleDto MapToDto(Article article, User? currentUser = null)
  {
    var isFavorited = currentUser != null && article.FavoritedBy.Any(u => u.Id == currentUser.Id);
    var isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;

    return new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      article.CreatedAt,
      article.UpdatedAt,
      isFavorited,
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        isFollowing
      )
    );
  }
}