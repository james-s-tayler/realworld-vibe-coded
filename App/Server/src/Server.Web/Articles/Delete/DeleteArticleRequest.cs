namespace Server.Web.Articles.Delete;

public class DeleteArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}
