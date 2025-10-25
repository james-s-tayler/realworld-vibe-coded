using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Update;

public record UpdateArticleCommand(
  string Slug,
  string? Title,
  string? Description,
  string? Body,
  Guid UserId,
  Guid CurrentUserId
) : ICommand<Article>;
