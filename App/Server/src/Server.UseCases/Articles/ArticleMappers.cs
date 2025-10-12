namespace Server.UseCases.Articles;

/// <summary>
/// Utility methods for Article-related operations
/// </summary>
public static class ArticleMappers
{
  /// <summary>
  /// Generates a URL-friendly slug from an article title
  /// </summary>
  public static string GenerateSlug(string title)
  {
    return title.ToLowerInvariant()
      .Replace(" ", "-")
      .Replace(".", "")
      .Replace(",", "")
      .Replace("!", "")
      .Replace("?", "")
      .Replace("'", "")
      .Replace("\"", "");
  }
}
