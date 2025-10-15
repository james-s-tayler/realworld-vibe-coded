using FluentValidation.Results;

namespace Server.Web.Articles;

public static class QueryParameterValidator
{
  public static class ValidationResults
  {
    public record ValidationResult(bool IsValid, List<ValidationFailure> Errors, int Limit, int Offset, string? Tag = null, string? Author = null, string? Favorited = null);
  }

  public static ValidationResults.ValidationResult ValidateListArticlesParameters(HttpRequest request)
  {
    var errors = new List<ValidationFailure>();
    var tagParam = request.Query["tag"].FirstOrDefault();
    var authorParam = request.Query["author"].FirstOrDefault();
    var favoritedParam = request.Query["favorited"].FirstOrDefault();
    var limitParam = request.Query["limit"].FirstOrDefault();
    var offsetParam = request.Query["offset"].FirstOrDefault();

    // Parse and validate limit
    int limit = 20;
    if (!string.IsNullOrEmpty(limitParam))
    {
      if (!int.TryParse(limitParam, out limit))
      {
        errors.Add(new ValidationFailure("limit", "must be a valid integer"));
      }
      else if (limit <= 0)
      {
        errors.Add(new ValidationFailure("limit", "must be greater than 0"));
      }
    }

    // Parse and validate offset
    int offset = 0;
    if (!string.IsNullOrEmpty(offsetParam))
    {
      if (!int.TryParse(offsetParam, out offset))
      {
        errors.Add(new ValidationFailure("offset", "must be a valid integer"));
      }
      else if (offset < 0)
      {
        errors.Add(new ValidationFailure("offset", "must be greater than or equal to 0"));
      }
    }

    // Validate string parameters for empty values
    if (tagParam == "")
    {
      errors.Add(new ValidationFailure("tag", "cannot be empty"));
    }

    if (authorParam == "")
    {
      errors.Add(new ValidationFailure("author", "cannot be empty"));
    }

    if (favoritedParam == "")
    {
      errors.Add(new ValidationFailure("favorited", "cannot be empty"));
    }

    return new ValidationResults.ValidationResult(
      !errors.Any(),
      errors,
      limit,
      offset,
      tagParam,
      authorParam,
      favoritedParam
    );
  }

  public static ValidationResults.ValidationResult ValidateFeedParameters(HttpRequest request)
  {
    var errors = new List<ValidationFailure>();
    var limitParam = request.Query["limit"].FirstOrDefault();
    var offsetParam = request.Query["offset"].FirstOrDefault();

    // Parse and validate limit
    int limit = 20;
    if (!string.IsNullOrEmpty(limitParam))
    {
      if (!int.TryParse(limitParam, out limit))
      {
        errors.Add(new ValidationFailure("limit", "must be a valid integer"));
      }
      else if (limit <= 0)
      {
        errors.Add(new ValidationFailure("limit", "must be greater than 0"));
      }
    }

    // Parse and validate offset
    int offset = 0;
    if (!string.IsNullOrEmpty(offsetParam))
    {
      if (!int.TryParse(offsetParam, out offset))
      {
        errors.Add(new ValidationFailure("offset", "must be a valid integer"));
      }
      else if (offset < 0)
      {
        errors.Add(new ValidationFailure("offset", "must be greater than or equal to 0"));
      }
    }

    return new ValidationResults.ValidationResult(!errors.Any(), errors, limit, offset);
  }
}
