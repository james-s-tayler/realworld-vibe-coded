using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;

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
    var query = BuildQuery(tag, author, favorited);

    var articles = await query
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .ToListAsync();

    // Get current user with following relationships if authenticated
    User? currentUser = null;
    if (currentUserId.HasValue)
    {
      currentUser = await _context.Users
        .Include(u => u.Following)
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == currentUserId.Value);
    }

    return articles.Select(a => MapToDto(a, currentUser));
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
