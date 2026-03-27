namespace Server.Web.Articles.Update;

public class UpdateArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;

  public UpdateArticleData Article { get; set; } = new();
}
