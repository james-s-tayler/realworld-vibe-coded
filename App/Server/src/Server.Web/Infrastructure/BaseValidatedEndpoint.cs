namespace Server.Web.Infrastructure;

/// <summary>
/// Base endpoint class that provides standardized validation error handling
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public abstract class BaseValidatedEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
  where TRequest : notnull, new()
{
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

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    });
    HttpContext.Response.WriteAsync(json).GetAwaiter().GetResult();
  }

  /// <summary>
  /// Helper method to write unauthorized response
  /// </summary>
  protected async Task WriteUnauthorizedResponseAsync(CancellationToken cancellationToken)
  {
    HttpContext.Response.StatusCode = 401;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { "Unauthorized" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}
