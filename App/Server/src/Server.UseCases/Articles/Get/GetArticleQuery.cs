using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Get;

public record GetArticleQuery(
  string Slug,
  Guid? CurrentUserId = null
) : IQuery<Article>;
