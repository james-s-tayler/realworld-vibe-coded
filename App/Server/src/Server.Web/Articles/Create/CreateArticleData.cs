namespace Server.Web.Articles.Create;

public class CreateArticleData
{
  public string Title { get; set; } = default!;

  public string Description { get; set; } = default!;

  public string Body { get; set; } = default!;

  public List<string>? TagList { get; set; }
}
