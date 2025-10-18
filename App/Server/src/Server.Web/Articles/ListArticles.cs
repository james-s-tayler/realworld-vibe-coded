using FluentValidation.Results;
using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication optional.
/// </remarks>
public class ListArticles(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticlesResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "List articles";
      s.Description = "List articles globally. Optional filters for tag, author, favorited user. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var validation = QueryParameterValidator.ValidateListArticlesParameters(HttpContext.Request);

    var errors = new List<string>();
    var tagParam = HttpContext.Request.Query["tag"].FirstOrDefault();
    var authorParam = HttpContext.Request.Query["author"].FirstOrDefault();
    var favoritedParam = HttpContext.Request.Query["favorited"].FirstOrDefault();
    var limitParam = HttpContext.Request.Query["limit"].FirstOrDefault();
    var offsetParam = HttpContext.Request.Query["offset"].FirstOrDefault();

    // Parse and validate limit
    int limit = 20;
    if (!string.IsNullOrEmpty(limitParam))
    {
      if (!int.TryParse(limitParam, out limit))
      {
        AddError(new ValidationFailure("limit", "limit must be a valid integer"));
      }
      else if (limit <= 0)
      {
        AddError(new ValidationFailure("limit", "limit must be greater than 0"));
      }
    }

    // Parse and validate offset
    int offset = 0;
    if (!string.IsNullOrEmpty(offsetParam))
    {
      if (!int.TryParse(offsetParam, out offset))
      {
        AddError(new ValidationFailure("offset", "offset must be a valid integer"));
      }
      else if (offset < 0)
      {
        AddError(new ValidationFailure("offset", "offset must be greater than or equal to 0"));
      }
    }

    // Validate string parameters for empty values
    if (tagParam == "")
    {
      AddError(new ValidationFailure("tag", "tag cannot be empty"));
    }

    if (authorParam == "")
    {
      AddError(new ValidationFailure("author", "author cannot be empty"));
    }

    if (favoritedParam == "")
    {
      AddError(new ValidationFailure("favorited", "favorited cannot be empty"));
    }

    ThrowIfAnyErrors();

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new ListArticlesQuery(
      validation.Tag,
      validation.Author,
      validation.Favorited,
      validation.Limit,
      validation.Offset,
      currentUserId), cancellationToken);

    await Send.ResultAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}
