using FastEndpoints;
using MediatR;
using Server.UseCases.Articles.List;

namespace Server.Web.Articles;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// Returns most recent articles globally, with optional filtering by tag, author, or favorited user.
/// </remarks>
public class ListArticles(IMediator _mediator) : EndpointWithoutRequest<ListArticlesResponse>
{
  public override void Configure()
  {
    Get("/api/articles");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "List articles";
      s.Description = "Returns most recent articles globally, with optional filtering by tag, author, or favorited user.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Extract query parameters manually
    var tag = Query<string?>("tag", isRequired: false);
    var author = Query<string?>("author", isRequired: false);
    var favorited = Query<string?>("favorited", isRequired: false);
    var limit = Query<int?>("limit", isRequired: false) ?? 20;
    var offset = Query<int?>("offset", isRequired: false) ?? 0;

    // Validate limits
    if (limit < 1) limit = 20;
    if (limit > 100) limit = 100;
    if (offset < 0) offset = 0;

    var query = new ListArticlesQuery(
      Tag: tag,
      Author: author,
      Favorited: favorited,
      Limit: limit,
      Offset: offset
    );

    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      var resultValue = result.Value;
      Response = new ListArticlesResponse
      {
        Articles = resultValue.Articles.Select(a => new ArticleResponse
        {
          Slug = a.Slug,
          Title = a.Title,
          Description = a.Description,
          Body = a.Body,
          TagList = a.TagList,
          CreatedAt = a.CreatedAt,
          UpdatedAt = a.UpdatedAt,
          Favorited = a.Favorited,
          FavoritesCount = a.FavoritesCount,
          Author = new AuthorResponse
          {
            Username = a.Author.Username,
            Bio = a.Author.Bio,
            Image = a.Author.Image,
            Following = a.Author.Following
          }
        }).ToList(),
        ArticlesCount = resultValue.ArticlesCount
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
