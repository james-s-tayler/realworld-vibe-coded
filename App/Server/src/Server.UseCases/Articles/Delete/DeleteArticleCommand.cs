namespace Server.UseCases.Articles.Delete;

public record DeleteArticleCommand(
  string Slug,
  Guid UserId
) : ICommand<Unit>;
