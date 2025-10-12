using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;

namespace Server.UseCases.Articles.Update;

public class UpdateArticleHandler(IRepository<Article> _articleRepository)
  : ICommandHandler<UpdateArticleCommand, Result<Article>>
{
  public async Task<Result<Article>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
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
      var newSlug = ArticleMappers.GenerateSlug(request.Title);
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

    return Result.Success(article);
  }

  private static string GenerateSlug(string title)
  {
    return ArticleMappers.GenerateSlug(title);
  }
}
