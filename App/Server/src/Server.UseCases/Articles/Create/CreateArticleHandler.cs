using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Create;

public class CreateArticleHandler : ICommandHandler<CreateArticleCommand, Result<ArticleDto>>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly IRepository<User> _userRepository;
  private readonly IRepository<Tag> _tagRepository;
  private readonly ILogger<CreateArticleHandler> _logger;

  public CreateArticleHandler(
    IRepository<Article> articleRepository,
    IRepository<User> userRepository,
    IRepository<Tag> tagRepository,
    ILogger<CreateArticleHandler> logger)
  {
    _articleRepository = articleRepository;
    _userRepository = userRepository;
    _tagRepository = tagRepository;
    _logger = logger;
  }

  public async Task<Result<ArticleDto>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Creating article '{Title}' by user {AuthorId}", request.Title, request.AuthorId);

    // Get the author
    var author = await _userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (author == null)
    {
      return Result.NotFound("Author not found");
    }

    try
    {
      // Create the article
      var article = new Article(request.Title, request.Description, request.Body, author);

      // Check for duplicate slug
      var existingArticle = await _articleRepository
        .FirstOrDefaultAsync(new ArticleBySlugSpec(article.Slug), cancellationToken);
      
      if (existingArticle != null)
      {
        return Result.Invalid(new ValidationError
        {
          Identifier = "slug",
          ErrorMessage = "has already been taken",
        });
      }

      // Add tags if provided
      if (request.TagList != null && request.TagList.Any())
      {
        foreach (var tagName in request.TagList.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
          // Find or create tag
          var existingTag = await _tagRepository
            .FirstOrDefaultAsync(new TagByNameSpec(tagName), cancellationToken);
          
          if (existingTag != null)
          {
            article.AddTag(existingTag);
          }
          else
          {
            var newTag = new Tag(tagName);
            await _tagRepository.AddAsync(newTag, cancellationToken);
            article.AddTag(newTag);
          }
        }
      }

      var createdArticle = await _articleRepository.AddAsync(article, cancellationToken);

      _logger.LogInformation("Article '{Title}' created successfully with ID {ArticleId}", 
        createdArticle.Title, createdArticle.Id);

      // Return the article DTO
      return Result.Success(new ArticleDto(
        createdArticle.Slug,
        createdArticle.Title,
        createdArticle.Description,
        createdArticle.Body,
        createdArticle.Tags.Select(t => t.Name).ToList(),
        DateTime.SpecifyKind(createdArticle.CreatedAt, DateTimeKind.Utc),
        DateTime.SpecifyKind(createdArticle.UpdatedAt, DateTimeKind.Utc),
        false, // favorited - TODO: implement when we have current user context
        createdArticle.FavoritesCount,
        new AuthorDto(
          author.Username,
          author.Bio ?? string.Empty,
          author.Image, // Keep null if null, don't convert to empty string
          false // following - TODO: implement when we have current user context
        )
      ));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating article '{Title}' by user {AuthorId}", request.Title, request.AuthorId);
      return Result.Error("An error occurred while creating the article");
    }
  }
}