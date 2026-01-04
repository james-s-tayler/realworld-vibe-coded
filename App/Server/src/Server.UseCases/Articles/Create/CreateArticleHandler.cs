using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.Core.IdentityAggregate;
using Server.Core.TagAggregate;
using Server.Core.TagAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  UserManager<ApplicationUser> userManager,
  IRepository<Article> articleRepository,
  IRepository<Author> authorRepository,
  IRepository<Tag> tagRepository)
  : ICommandHandler<CreateArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    // Get the user to verify they exist
    var user = await userManager.FindByIdAsync(request.AuthorId.ToString());
    if (user == null)
    {
      return Result<Article>.ErrorMissingRequiredEntity(typeof(ApplicationUser), request.AuthorId);
    }

    // Get or create the author
    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorByUserIdSpec(request.AuthorId), cancellationToken);

    if (author == null)
    {
      author = new Author(user.Id, user.UserName!, user.Bio, user.Image);
      await authorRepository.AddAsync(author, cancellationToken);
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
