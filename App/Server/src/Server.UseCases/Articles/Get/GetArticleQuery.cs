namespace Server.UseCases.Articles.Get;

public record GetArticleQuery(
  string Slug,
  int? CurrentUserId = null
) : IQuery<Result<ArticleResponse>>;
