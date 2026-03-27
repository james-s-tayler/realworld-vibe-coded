using Server.Web.Profiles;

namespace Server.Web.Articles;

public class ArticleDto
{
  public string Slug { get; set; } = string.Empty;

  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string? Body { get; set; }

  public List<string> TagList { get; set; } = [];

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public bool Favorited { get; set; }

  public int FavoritesCount { get; set; }

  public ProfileDto Author { get; set; } = default!;
}
