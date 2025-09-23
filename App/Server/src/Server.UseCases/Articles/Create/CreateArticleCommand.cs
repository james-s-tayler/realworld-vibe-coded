using Ardalis.Result;
using Ardalis.SharedKernel;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Create;

public record CreateArticleCommand(
  int AuthorId,
  string Title,
  string Description,
  string Body,
  List<string>? TagList
) : ICommand<Result<ArticleDto>>;