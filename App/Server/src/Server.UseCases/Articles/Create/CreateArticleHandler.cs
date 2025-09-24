using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler(IRepository<User> _userRepository, IRepository<Article> _articleRepository, IRepository<Tag> _tagRepository)
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
      return Result.Error("An article with this title already exists");
    }

    // Create the article
    var article = new Article(request.Title, request.Description, request.Body, author);

    // Handle tags
    foreach (var tagName in request.TagList ?? new List<string>())
    {
      if (string.IsNullOrWhiteSpace(tagName) || tagName.Contains(","))
      {
        return Result.Error("Invalid tag format");
      }

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
