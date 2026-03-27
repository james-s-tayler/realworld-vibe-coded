using System.Text.RegularExpressions;

namespace Server.UseCases.Articles;

public static partial class SlugHelper
{
  public static string GenerateSlug(string title)
  {
    var slug = title.ToLowerInvariant();
    slug = NonSlugChars().Replace(slug, string.Empty);
    slug = Whitespace().Replace(slug, "-");
    slug = slug.Trim('-');
    return slug;
  }

  [GeneratedRegex(@"[^a-z0-9\s\-_]")]
  private static partial Regex NonSlugChars();

  [GeneratedRegex(@"\s+")]
  private static partial Regex Whitespace();
}
