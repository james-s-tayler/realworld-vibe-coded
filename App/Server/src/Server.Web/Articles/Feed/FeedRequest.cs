namespace Server.Web.Articles.Feed;

public class FeedRequest
{
  [QueryParam]
  public int Limit { get; set; } = 20;

  [QueryParam]
  public int Offset { get; set; } = 0;
}
