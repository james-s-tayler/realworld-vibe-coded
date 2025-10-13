using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Create;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Comments;

/// <summary>
/// Create comment for an article
/// </summary>
/// <remarks>
/// Create a new comment for an article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<CreateCommentRequest, CommentResponse>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/comments");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Create comment";
      s.Description = "Create a new comment for an article. Authentication required.";
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      var propertyName = failure.PropertyName.ToLower();
      if (propertyName.Contains('.'))
      {
        propertyName = propertyName.Split('.').Last();
      }

      errorBody.Add($"{propertyName} {failure.ErrorMessage}");
    }

    Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
    }, 422).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(CreateCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await _mediator.Send(new CreateCommentCommand(slug, request.Comment.Body, userId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
      await SendAsync(Response, 201);
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Article not found" } }
      }, 422);
      return;
    }

    await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 422);
  }
}

public class CreateCommentRequest
{
  public CreateCommentDto Comment { get; set; } = default!;
}

public class CreateCommentDto
{
  public string Body { get; set; } = string.Empty;
}
