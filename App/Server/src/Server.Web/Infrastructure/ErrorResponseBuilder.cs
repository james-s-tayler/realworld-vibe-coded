using System.Text.Json;
using Ardalis.Result;
using FluentValidation.Results;

namespace Server.Web.Infrastructure;

/// <summary>
/// Builds standardized error responses that comply with RealWorld API format
/// </summary>
public static class ErrorResponseBuilder
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  /// <summary>
  /// Create an error response from validation failures
  /// </summary>
  public static string CreateValidationErrorResponse(IEnumerable<ValidationFailure> failures)
  {
    var errorBody = new List<string>();

    foreach (var failure in failures)
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

    return JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    }, JsonOptions);
  }

  /// <summary>
  /// Create an error response from Ardalis Result validation errors
  /// </summary>
  public static string CreateValidationErrorResponse(IEnumerable<ValidationError> validationErrors)
  {
    var errorBody = new List<string>();
    foreach (var error in validationErrors)
    {
      errorBody.Add($"{error.Identifier} {error.ErrorMessage}");
    }

    return JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    }, JsonOptions);
  }

  /// <summary>
  /// Create an error response from general Result errors
  /// </summary>
  public static string CreateErrorResponse(IEnumerable<string> errors)
  {
    return JsonSerializer.Serialize(new
    {
      errors = new { body = errors.ToArray() }
    }, JsonOptions);
  }

  /// <summary>
  /// Create an error response from a single error message
  /// </summary>
  public static string CreateErrorResponse(string errorMessage)
  {
    return CreateErrorResponse(new[] { errorMessage });
  }

  /// <summary>
  /// Create unauthorized error response
  /// </summary>
  public static string CreateUnauthorizedResponse()
  {
    return CreateErrorResponse("Unauthorized");
  }
}
