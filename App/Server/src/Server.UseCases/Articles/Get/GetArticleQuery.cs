using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Get;

public record GetArticleQuery(
  string Slug,
  Guid? CurrentUserId = null
) : IQuery<Article>;
