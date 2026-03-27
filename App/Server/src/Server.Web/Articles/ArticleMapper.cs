using Server.UseCases.Articles;
using Server.Web.Profiles;

namespace Server.Web.Articles;

public class ArticleMapper : ResponseMapper<ArticleResponse, ArticleResult>
{
  public override Task<ArticleResponse> FromEntityAsync(ArticleResult result, CancellationToken ct)
  {
    var response = new ArticleResponse
    {
      Article = new ArticleDto
      {
        Slug = result.Article.Slug,
        Title = result.Article.Title,
        Description = result.Article.Description,
        Body = result.Article.Body,
        TagList = result.Article.TagList,
        CreatedAt = result.Article.CreatedAt,
        UpdatedAt = result.Article.UpdatedAt,
        Favorited = result.Favorited,
        FavoritesCount = result.FavoritesCount,
        Author = new ProfileDto
        {
          Username = result.Article.Author.UserName ?? string.Empty,
          Bio = result.Article.Author.Bio,
          Image = result.Article.Author.Image,
          Following = result.AuthorFollowing,
        },
      },
    };

    return Task.FromResult(response);
  }
}
