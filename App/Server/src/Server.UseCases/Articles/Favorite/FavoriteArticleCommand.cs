namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(
  string Slug,
  int UserId,
  int CurrentUserId
) : ICommand<Result<ArticleResponse>>;
