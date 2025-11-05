namespace Server.Core.ArticleAggregate.Dtos;

public record CommentsResponse(
  List<CommentDto> Comments
);
