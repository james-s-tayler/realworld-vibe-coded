namespace Server.Web.Articles.Favorite;

public class FavoriteRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
