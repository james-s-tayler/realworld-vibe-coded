using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Update;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Update;

/// <summary>
/// Update article
/// </summary>
/// <remarks>
/// Updates an existing article. Authentication required. User must be the author.
/// </remarks>
public class Update(IMediator _mediator, IUserContext userContext) : Endpoint<UpdateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Put("/api/articles/{slug}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Update article";
      s.Description = "Updates an existing article. Authentication required. User must be the author.";
    });
  }

  public override async Task HandleAsync(UpdateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UpdateArticleCommand(
      request.Slug,
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      userId,
      userId), cancellationToken);

    await Send.ResultMapperAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}

public class UpdateArticleRequest
{
  public string Slug { get; set; } = string.Empty;
  public UpdateArticleData Article { get; set; } = new();
}

public class UpdateArticleData
{
  public string? Title { get; set; } = null;
  public string? Description { get; set; } = null;
  public string? Body { get; set; } = null;
}
