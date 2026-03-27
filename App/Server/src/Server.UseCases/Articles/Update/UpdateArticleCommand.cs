using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Update;

public record UpdateArticleCommand(
  string Slug,
  string? Title,
  string? Description,
  string? Body,
  Guid CurrentUserId) : ICommand<ArticleResult>;
