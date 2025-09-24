using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Update;

public class UpdateArticleHandler(IRepository<Article> _articleRepository, IRepository<User> _userRepository)
  : ICommandHandler<UpdateArticleCommand, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    if (article.AuthorId != request.UserId)
    {
      return Result.Forbidden("You can only update your own articles");
    }

    // Check for duplicate slug if title changed
    if (request.Title != null && article.Title != request.Title)
    {
      var newSlug = GenerateSlug(request.Title);
      if (newSlug != article.Slug)
      {
        var existingArticle = await _articleRepository.FirstOrDefaultAsync(
          new ArticleBySlugSpec(newSlug), cancellationToken);

        if (existingArticle != null)
        {
          return Result.Error("slug has already been taken");
        }
      }
    }

    // Update only the fields that are provided (not null)
    var newTitle = request.Title ?? article.Title;
    var newDescription = request.Description ?? article.Description;
    var newBody = request.Body ?? article.Body;

    article.Update(newTitle, newDescription, newBody);
    await _articleRepository.UpdateAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    // Load current user with following relationships to check following status
    var currentUserWithFollowing = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.CurrentUserId), cancellationToken);

    var isFollowing = currentUserWithFollowing?.IsFollowing(article.AuthorId) ?? false;

    // Check if current user has favorited this article
    var isFavorited = article.FavoritedBy.Any(u => u.Id == request.CurrentUserId);

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

  private static string GenerateSlug(string title)
  {
    return title.ToLowerInvariant()
      .Replace(" ", "-")
      .Replace(".", "")
      .Replace(",", "")
      .Replace("!", "")
      .Replace("?", "")
      .Replace("'", "")
      .Replace("\"", "");
  }
}
