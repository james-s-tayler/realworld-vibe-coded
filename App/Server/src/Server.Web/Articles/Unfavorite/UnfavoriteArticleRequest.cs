namespace Server.Web.Articles.Unfavorite;

public class UnfavoriteArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
