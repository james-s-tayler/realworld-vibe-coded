using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Create;

public record CreateArticleCommand(
  string Title,
  string Description,
  string Body,
  List<string> TagList,
  Guid AuthorId,
  Guid? CurrentUserId = null
) : ICommand<Article>;
