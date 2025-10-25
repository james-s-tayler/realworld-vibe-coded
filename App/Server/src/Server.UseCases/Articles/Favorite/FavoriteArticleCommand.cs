using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(
  string Slug,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
