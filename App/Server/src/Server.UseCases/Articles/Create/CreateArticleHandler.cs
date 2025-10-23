using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  IRepository<User> _userRepository,
  IRepository<Article> _articleRepository,
  IRepository<Tag> _tagRepository)
  : ICommandHandler<CreateArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    // Get the author
    var author = await _userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (author == null)
    {
      return Result.Error("Author not found");
    }

    // Check for duplicate slug
    var slug = Article.GenerateSlug(request.Title);
    var existingArticle = await _articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(slug), cancellationToken);

    if (existingArticle != null)
    {
      return Result.Invalid(new ValidationError("slug", "has already been taken"));
    }

    // Create the article
    var article = new Article(request.Title, request.Description, request.Body, author);

    // Handle tags - validation is now done at the endpoint level
    foreach (var tagName in request.TagList ?? new List<string>())
    {
      var existingTag = await _tagRepository.FirstOrDefaultAsync(
        new TagByNameSpec(tagName), cancellationToken);

      if (existingTag == null)
      {
        existingTag = new Tag(tagName);
        await _tagRepository.AddAsync(existingTag, cancellationToken);
      }

      article.AddTag(existingTag);
    }

    await _articleRepository.AddAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return Result.Created(article);
  }
}
