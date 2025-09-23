namespace Server.Web.Articles;

public class ListArticlesRequest
{
  public const string Route = "/api/articles";
  public string? Tag { get; set; }
  public string? Author { get; set; }
  public string? Favorited { get; set; }
  public int Limit { get; set; } = 20;
  public int Offset { get; set; } = 0;
}

public class ListArticlesResponse
{
  public List<ArticleResponse> Articles { get; set; } = new();
  public int ArticlesCount { get; set; }
}

public class ArticleResponse
{
  public string Slug { get; set; } = default!;
  public string Title { get; set; } = default!;
  public string Description { get; set; } = default!;
  public string Body { get; set; } = default!;
  public List<string> TagList { get; set; } = new();
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public bool Favorited { get; set; }
  public int FavoritesCount { get; set; }
  public AuthorResponse Author { get; set; } = default!;
}

public class AuthorResponse
{
  public string Username { get; set; } = default!;
  public string Bio { get; set; } = default!;
  public string? Image { get; set; }
  public bool Following { get; set; }
}
