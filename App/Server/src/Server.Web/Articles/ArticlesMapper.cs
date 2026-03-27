using Server.UseCases.Articles;
using Server.Web.Profiles;

namespace Server.Web.Articles;

public class ArticlesMapper : ResponseMapper<ArticlesResponse, ArticlesListResult>
{
  public override Task<ArticlesResponse> FromEntityAsync(ArticlesListResult result, CancellationToken ct)
  {
    var response = new ArticlesResponse
    {
      Articles = result.Articles.Select(r => new ArticleDto
      {
        Slug = r.Article.Slug,
        Title = r.Article.Title,
        Description = r.Article.Description,
        TagList = r.Article.TagList,
        CreatedAt = r.Article.CreatedAt,
        UpdatedAt = r.Article.UpdatedAt,
        Favorited = r.Favorited,
        FavoritesCount = r.FavoritesCount,
        Author = new ProfileDto
        {
          Username = r.Article.Author.UserName ?? string.Empty,
          Bio = r.Article.Author.Bio,
          Image = r.Article.Author.Image,
          Following = r.AuthorFollowing,
        },
      }).ToList(),
      ArticlesCount = result.ArticlesCount,
    };

    return Task.FromResult(response);
  }
}
