namespace Server.Web.Articles.List;

public class ListArticlesRequest
{
  [QueryParam]
  public string? Tag { get; set; }

  [QueryParam]
  public string? Author { get; set; }

  [QueryParam]
  public string? Favorited { get; set; }

  [QueryParam]
  public int Limit { get; set; } = 20;

  [QueryParam]
  public int Offset { get; set; } = 0;
}
