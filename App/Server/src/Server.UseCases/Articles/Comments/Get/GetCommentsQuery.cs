using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Comments.Get;

public record GetCommentsQuery(
  string Slug,
  int? CurrentUserId = null
) : IQuery<Result<CommentsResponse>>;
