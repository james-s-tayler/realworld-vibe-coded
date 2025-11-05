using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Update;

public class UpdateArticleHandler(IRepository<Article> _articleRepository)
  : ICommandHandler<UpdateArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(request.Slug);
    }

    if (article.AuthorId != request.UserId)
    {
      return Result<Article>.Forbidden(new ErrorDetail("Forbidden", "You can only update your own articles"));
    }

    // Check for duplicate slug if title changed
    if (request.Title != null && article.Title != request.Title)
    {
      var newSlug = Article.GenerateSlug(request.Title);
      if (newSlug != article.Slug)
      {
        var existingArticle = await _articleRepository.FirstOrDefaultAsync(
          new ArticleBySlugSpec(newSlug), cancellationToken);

        if (existingArticle != null)
        {
          return Result<Article>.Invalid(new ErrorDetail("slug", "duplicate slug"));
        }
      }
    }

    // Update only the fields that are provided (not null)
    var newTitle = request.Title ?? article.Title;
    var newDescription = request.Description ?? article.Description;
    var newBody = request.Body ?? article.Body;

    article.Update(newTitle, newDescription, newBody);
    await _articleRepository.UpdateAsync(article, cancellationToken);

    return Result<Article>.Success(article);
  }
}
