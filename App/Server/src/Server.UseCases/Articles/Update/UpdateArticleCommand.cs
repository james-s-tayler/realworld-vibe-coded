using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Update;

public record UpdateArticleCommand(
  string Slug,
  string? Title,
  string? Description,
  string? Body,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
