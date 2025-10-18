namespace Server.Web.Articles.List;

public class ListArticlesRequest
{
  [BindFrom("tag")]
  public string? Tag { get; set; }

  [BindFrom("author")]
  public string? Author { get; set; }

  [BindFrom("favorited")]
  public string? Favorited { get; set; }

  [BindFrom("limit")]
  public int Limit { get; set; } = 20;

  [BindFrom("offset")]
  public int Offset { get; set; } = 0;
}
