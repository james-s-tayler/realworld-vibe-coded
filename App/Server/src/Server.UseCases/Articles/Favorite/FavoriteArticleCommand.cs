using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(
  string Slug,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
