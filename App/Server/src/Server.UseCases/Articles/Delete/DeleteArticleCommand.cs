using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Delete;

public record DeleteArticleCommand(
  string Slug,
  Guid UserId
) : ICommand<Article>;
