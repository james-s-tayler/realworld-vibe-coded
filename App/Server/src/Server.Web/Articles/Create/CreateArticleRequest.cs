namespace Server.Web.Articles.Create;

public class CreateArticleRequest
{
  public ArticleData Article { get; set; } = new();
}
