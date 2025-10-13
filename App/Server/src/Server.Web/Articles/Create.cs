using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Create article
/// </summary>
/// <remarks>
/// Creates a new article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<CreateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles");
    AuthSchemes("Token");
    DontAutoTag();
    Summary(s =>
    {
      s.Summary = "Create article";
      s.Description = "Creates a new article. Authentication required.";
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      // Handle nested properties like Article.Title -> title
      var propertyName = failure.PropertyName.ToLower();
      if (propertyName.Contains('.'))
      {
        propertyName = propertyName.Split('.').Last();
      }

      // Handle array indexing for tags like Article.TagList[0] -> taglist[0]
      if (propertyName.Contains("taglist["))
      {
        // Already in the right format, just ensure lowercase
        propertyName = propertyName.Replace("taglist", "taglist");
      }

      errorBody.Add($"{propertyName} {failure.ErrorMessage}");
    }

    Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
    }, 422).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new CreateArticleCommand(
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      request.Article.TagList ?? new List<string>(),
      userId,
      userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
      await SendAsync(Response, 201);
      return;
    }

    await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 422);
  }
}
