using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.IdentityAggregate;
using Server.Core.TagAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  IRepository<Article> articleRepo,
  IRepository<Tag> tagRepo,
  UserManager<ApplicationUser> userManager)
  : ICommandHandler<CreateArticleCommand, ArticleResult>
{
  public async Task<Result<ArticleResult>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    var slug = SlugHelper.GenerateSlug(request.Title);

    var slugExists = await articleRepo.AnyAsync(new ArticleBySlugSpec(slug), cancellationToken);
    if (slugExists)
    {
      return Result<ArticleResult>.Invalid(new ErrorDetail("slug", "has already been taken."));
    }

    var author = await userManager.FindByIdAsync(request.AuthorId.ToString());
    if (author == null)
    {
      return Result<ArticleResult>.NotFound();
    }

    var article = new Article
    {
      Slug = slug,
      Title = request.Title,
      Description = request.Description,
      Body = request.Body,
      AuthorId = request.AuthorId,
      Author = author,
      TagList = request.TagList,
    };

    foreach (var tagName in request.TagList)
    {
      var tag = await tagRepo.FirstOrDefaultAsync(new TagByNameSpec(tagName), cancellationToken);
      if (tag == null)
      {
        tag = new Tag { Name = tagName };
        await tagRepo.AddAsync(tag, cancellationToken);
      }

      article.Tags.Add(tag);
    }

    await articleRepo.AddAsync(article, cancellationToken);

    return Result<ArticleResult>.Created(new ArticleResult(article, false, 0, false));
  }
}
