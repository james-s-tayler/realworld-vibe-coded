using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(
  string Slug,
  int UserId,
  int CurrentUserId
) : ICommand<Article>;
