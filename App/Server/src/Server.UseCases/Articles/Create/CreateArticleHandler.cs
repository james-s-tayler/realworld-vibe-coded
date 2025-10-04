using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  IRepository<User> _userRepository,
  IRepository<Article> _articleRepository,
  IRepository<Tag> _tagRepository)
  : ICommandHandler<CreateArticleCommand, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    // Get the author
    var author = await _userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (author == null)
    {
      return Result.Error("Author not found");
    }

    // Check for duplicate slug
    var slug = ArticleMappers.GenerateSlug(request.Title);
    var existingArticle = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(slug), cancellationToken);

    if (existingArticle != null)
    {
      return Result.Error("slug has already been taken");
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

    // Get current user with following relationships if authenticated
    var currentUser = request.CurrentUserId.HasValue ?
        await _userRepository.FirstOrDefaultAsync(new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken) : null;

    var articleDto = ArticleMappers.MapToDto(article, currentUser, false); // Always false for newly created articles

    return Result.Success(new ArticleResponse { Article = articleDto });
  }

  private static string GenerateSlug(string title)
  {
    return ArticleMappers.GenerateSlug(title);
  }
}
