using Ardalis.Result;
using MediatR;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Comments.Create;

public record CreateCommentCommand(
  string Slug, 
  string Body, 
  int AuthorId,
  int? CurrentUserId = null
) : IRequest<Result<CommentResponse>>;
