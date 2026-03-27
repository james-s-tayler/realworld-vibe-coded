namespace Server.Web.Comments.List;

public class ListCommentsRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
