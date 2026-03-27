namespace Server.Web.Articles.Get;

public class GetArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
