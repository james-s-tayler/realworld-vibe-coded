using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;

namespace Server.UseCases.Articles.Update;

public class UpdateArticleHandler(IRepository<Article> _articleRepository)
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
    if (article.Title != request.Title)
    {
      var newSlug = GenerateSlug(request.Title);
      if (newSlug != article.Slug)
      {
        var existingArticle = await _articleRepository.FirstOrDefaultAsync(
          new ArticleBySlugSpec(newSlug), cancellationToken);

        if (existingArticle != null)
        {
          return Result.Error("An article with this title already exists");
        }
      }
    }

    article.Update(request.Title, request.Description, request.Body);
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
      false, // TODO: Check if current user favorited
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
