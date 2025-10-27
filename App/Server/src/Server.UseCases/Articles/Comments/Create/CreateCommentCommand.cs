using Server.Core.ArticleAggregate.Dtos;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Comments.Create;

public record CreateCommentCommand(
  string Slug,
  string Body,
  Guid AuthorId,
  Guid? CurrentUserId = null
) : ICommand<CommentResponse>;
