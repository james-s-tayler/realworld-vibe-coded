namespace Server.Web.Articles;

public class ArticlesResponse
{
  public List<ArticleDto> Articles { get; set; } = [];

  public int ArticlesCount { get; set; }
}
