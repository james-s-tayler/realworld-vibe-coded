namespace Server.Web.Articles;

public class ListArticlesRequest
{
  /// <summary>
  /// Filter by tag
  /// </summary>
  public string? Tag { get; set; }

  /// <summary>
  /// Filter by author username
  /// </summary>
  public string? Author { get; set; }

  /// <summary>
  /// Filter by username of user who favorited
  /// </summary>
  public string? Favorited { get; set; }

  /// <summary>
  /// Number of articles to return (default 20)
  /// </summary>
  public int Limit { get; set; } = 20;

  /// <summary>
  /// Number of articles to skip (default 0)
  /// </summary>
  public int Offset { get; set; } = 0;
}
