namespace Server.UseCases.Articles.Create;

public record CreateArticleCommand(
  string Title,
  string Description,
  string Body,
  List<string> TagList,
  int AuthorId
) : ICommand<Result<ArticleResponse>>;
