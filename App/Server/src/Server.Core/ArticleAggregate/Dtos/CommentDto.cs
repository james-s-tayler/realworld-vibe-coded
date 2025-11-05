namespace Server.Core.ArticleAggregate.Dtos;

public record CommentDto(
  Guid Id,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  string Body,
  AuthorDto Author
);
