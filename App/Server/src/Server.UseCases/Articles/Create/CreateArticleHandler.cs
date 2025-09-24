using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(
  IRepository<User> _userRepository, 
  IRepository<Article> _articleRepository, 
  IRepository<Tag> _tagRepository,
  IRepository<UserFollowing> _userFollowingRepository)
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
    var slug = GenerateSlug(request.Title);
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

    // Check if current user is following the article author
    var currentUser = await _userRepository.GetByIdAsync(request.CurrentUserId ?? 0, cancellationToken);
    var isFollowing = currentUser != null && currentUser.Id != author.Id &&
                     await _userFollowingRepository.AnyAsync(
                       new IsFollowingSpec(currentUser.Id, author.Id), 
                       cancellationToken);

    // Check if current user has favorited the article (always false for newly created articles)
    var isFavorited = false;

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
