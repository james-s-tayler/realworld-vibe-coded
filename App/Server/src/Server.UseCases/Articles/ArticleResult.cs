using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles;

public record ArticleResult(
  Article Article,
  bool Favorited,
  int FavoritesCount,
  bool AuthorFollowing);
