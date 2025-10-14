namespace Server.Web.Articles;

public class ListArticlesRequest
{
  public string? Tag { get; set; }
  public string? Author { get; set; }
  public string? Favorited { get; set; }
  public int Limit { get; set; } = 20;
  public int Offset { get; set; } = 0;
}
