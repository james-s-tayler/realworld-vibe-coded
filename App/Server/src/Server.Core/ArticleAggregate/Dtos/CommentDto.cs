namespace Server.Core.ArticleAggregate.Dtos;

public record CommentDto(
  int Id,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  string Body,
  AuthorDto Author
);

public record CommentsResponse(
  List<CommentDto> Comments
);

public record CommentResponse(
  CommentDto Comment
);
