using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.TagAggregate;
using Server.Core.TagAggregate.Specifications;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  IRepository<User> userRepository,
  IRepository<Article> articleRepository,
  IRepository<Tag> tagRepository)
  : ICommandHandler<CreateArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    // Get the author
    var author = await userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (author == null)
    {
      return Result<Article>.ErrorMissingRequiredEntity(typeof(User), request.AuthorId);
    }

    // Check for duplicate slug
    var slug = Article.GenerateSlug(request.Title);
    var existingArticle = await articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(slug), cancellationToken);

    if (existingArticle != null)
    {
      return Result<Article>.Invalid(new ErrorDetail("slug", "has already been taken"));
    }

    // Create the article
    var article = new Article(request.Title, request.Description, request.Body, author);

    // Handle tags - validation is now done at the endpoint level
    foreach (var tagName in request.TagList ?? new List<string>())
    {
      var existingTag = await tagRepository.FirstOrDefaultAsync(
        new TagByNameSpec(tagName), cancellationToken);

      if (existingTag == null)
      {
        existingTag = new Tag(tagName);
        await tagRepository.AddAsync(existingTag, cancellationToken);
      }

      article.AddTag(existingTag);
    }

    await articleRepository.AddAsync(article, cancellationToken);

    return Result<Article>.Created(article);
  }
}
