namespace Server.Web.Articles.Unfavorite;

public class UnfavoriteRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
