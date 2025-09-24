using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Infrastructure.Data;

namespace Server.Infrastructure.Data.Queries;

public class ListArticlesQueryService(AppDbContext _context) : IListArticlesQueryService
{
  public async Task<IEnumerable<ArticleDto>> ListAsync(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0)
  {
    var query = BuildQuery(tag, author, favorited);

    var articles = await query
      .Skip(offset)
      .Take(Math.Min(limit, 100))
      .AsNoTracking()
      .ToListAsync();

    return articles.Select(MapToDto);
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

  private static ArticleDto MapToDto(Article article)
  {
    return new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      article.CreatedAt,
      article.UpdatedAt,
      false, // TODO: Check if current user favorited
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        false // TODO: Check if current user follows
      )
    );
  }
}
