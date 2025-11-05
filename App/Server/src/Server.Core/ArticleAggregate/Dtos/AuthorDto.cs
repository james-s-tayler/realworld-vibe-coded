namespace Server.Core.ArticleAggregate.Dtos;

public record AuthorDto(
  string Username,
  string Bio,
  string? Image,
  bool Following
);
