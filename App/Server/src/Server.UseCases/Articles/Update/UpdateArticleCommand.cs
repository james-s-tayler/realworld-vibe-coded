namespace Server.UseCases.Articles.Update;

public record UpdateArticleCommand(
  string Slug,
  string Title,
  string Description,
  string Body,
  int UserId
) : ICommand<Result<ArticleResponse>>;