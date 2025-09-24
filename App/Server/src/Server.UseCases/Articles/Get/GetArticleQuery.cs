namespace Server.UseCases.Articles.Get;

public record GetArticleQuery(
  string Slug
) : IQuery<Result<ArticleResponse>>;
