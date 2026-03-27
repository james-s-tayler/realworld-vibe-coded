namespace Server.Web.Articles.Feed;

public class FeedArticlesRequest
{
  [QueryParam]
  public int Limit { get; set; } = 20;

  [QueryParam]
  public int Offset { get; set; }
}
