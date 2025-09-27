namespace Server.Web.Articles;

public class FeedRequest
{
  /// <summary>
  /// Number of articles to return (default 20)
  /// </summary>
  public int Limit { get; set; } = 20;

  /// <summary>
  /// Number of articles to skip (default 0)
  /// </summary>
  public int Offset { get; set; } = 0;
}
