namespace Server.Web.Articles.Favorite;

public class FavoriteArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
