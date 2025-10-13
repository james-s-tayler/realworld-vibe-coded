namespace Server.Web.Infrastructure;

/// <summary>
/// Base endpoint class that provides standardized helper methods
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public abstract class BaseValidatedEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
  where TRequest : notnull, new()
{
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

/// <summary>
/// Base endpoint class that provides standardized helper methods with mapper support
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
/// <typeparam name="TMapper">Mapper type</typeparam>
public abstract class BaseValidatedEndpoint<TRequest, TResponse, TMapper> : Endpoint<TRequest, TResponse, TMapper>
  where TRequest : notnull, new()
  where TResponse : notnull
  where TMapper : class, IMapper
{
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
