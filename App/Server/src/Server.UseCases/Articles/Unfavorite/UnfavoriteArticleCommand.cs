using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Unfavorite;

public record UnfavoriteArticleCommand(
  string Slug,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
