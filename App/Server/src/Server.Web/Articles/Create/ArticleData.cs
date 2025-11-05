namespace Server.Web.Articles.Create;

public class ArticleData
{
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public List<string>? TagList { get; set; }
}
