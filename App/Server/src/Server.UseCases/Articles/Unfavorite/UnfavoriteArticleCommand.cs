namespace Server.UseCases.Articles.Unfavorite;

public record UnfavoriteArticleCommand(
  string Slug,
  int UserId
) : ICommand<Result<ArticleResponse>>;
