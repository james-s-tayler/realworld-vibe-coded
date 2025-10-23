namespace Server.UseCases.Articles.Delete;

public record DeleteArticleCommand(
  string Slug,
  int UserId
) : ICommand<Result<Unit>>;
