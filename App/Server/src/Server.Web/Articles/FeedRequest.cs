namespace Server.Web.Articles;

public class FeedRequest
{
  public int Limit { get; set; } = 20;
  public int Offset { get; set; } = 0;
}
