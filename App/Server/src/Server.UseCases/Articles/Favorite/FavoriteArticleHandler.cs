using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler(IRepository<Article> _articleRepository, IRepository<User> _userRepository)
  : ICommandHandler<FavoriteArticleCommand, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(FavoriteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user == null)
    {
      return Result.Error("User not found");
    }

    article.AddToFavorites(user);
    await _articleRepository.UpdateAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    var articleDto = new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      article.CreatedAt,
      article.UpdatedAt,
      true, // User just favorited this article
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        false // TODO: Check if current user follows
      )
    );

    return Result.Success(new ArticleResponse { Article = articleDto });
  }
}