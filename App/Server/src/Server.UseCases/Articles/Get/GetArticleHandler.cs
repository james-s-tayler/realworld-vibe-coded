using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(
  IRepository<Article> _articleRepository,
  IRepository<User> _userRepository,
  IRepository<UserFollowing> _userFollowingRepository)
  : IQueryHandler<GetArticleQuery, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Check if current user is following the article author
    var currentUser = await _userRepository.GetByIdAsync(request.CurrentUserId ?? 0, cancellationToken);
    var isFollowing = currentUser != null && currentUser.Id != article.Author.Id &&
                     await _userFollowingRepository.AnyAsync(
                       new IsFollowingSpec(currentUser.Id, article.Author.Id), 
                       cancellationToken);

    // Check if current user has favorited the article
    var isFavorited = false; // TODO: Implement favorites relationship

    var articleDto = new ArticleDto(
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

    return Result.Success(new ArticleResponse { Article = articleDto });
  }
}
