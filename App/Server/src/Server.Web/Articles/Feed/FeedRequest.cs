namespace Server.Web.Articles.Feed;

public class FeedRequest
{
  [BindFrom("limit")]
  public int? Limit { get; set; } = 20;
  [BindFrom("offset")]
  public int? Offset { get; set; } = 0;
}
