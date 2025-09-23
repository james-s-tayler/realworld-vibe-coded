using FastEndpoints;
using MediatR;
using Server.UseCases.Tags.List;

namespace Server.Web.Tags;

/// <summary>
/// Get tags
/// </summary>
/// <remarks>
/// Returns all tags.
/// </remarks>
public class ListTags(IMediator _mediator) : EndpointWithoutRequest<ListTagsResponse>
{
  public override void Configure()
  {
    Get(ListTagsRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get tags";
      s.Description = "Returns all tags.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var query = new ListTagsQuery();
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      var resultValue = result.Value;
      Response = new ListTagsResponse
      {
        Tags = resultValue.Tags
      };
      return;
    }

    HttpContext.Response.StatusCode = 500;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Internal server error" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}
