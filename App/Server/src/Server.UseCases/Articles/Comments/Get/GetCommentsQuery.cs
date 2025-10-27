using Server.Core.ArticleAggregate.Dtos;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Comments.Get;

public record GetCommentsQuery(
  string Slug,
  Guid? CurrentUserId = null
) : IQuery<CommentsResponse>;
