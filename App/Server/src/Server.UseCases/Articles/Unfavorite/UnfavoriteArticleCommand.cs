using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Unfavorite;

public record UnfavoriteArticleCommand(
  string Slug,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
