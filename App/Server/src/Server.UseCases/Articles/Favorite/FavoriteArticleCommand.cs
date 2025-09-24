namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(
  string Slug,
  int UserId
) : ICommand<Result<ArticleResponse>>;
