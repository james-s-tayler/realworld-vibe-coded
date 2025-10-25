using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Create;

public record CreateArticleCommand(
  string Title,
  string Description,
  string Body,
  List<string> TagList,
  Guid AuthorId,
  Guid? CurrentUserId = null
) : ICommand<Article>;
