using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Comments.Create;

public record CreateCommentCommand(
  string Slug,
  string Body,
  Guid AuthorId,
  Guid? CurrentUserId = null
) : ICommand<CommentResponse>;
