using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Unfavorite;

public record UnfavoriteArticleCommand(
  string Slug,
  int UserId,
  int CurrentUserId
) : ICommand<Result<Article>>;
