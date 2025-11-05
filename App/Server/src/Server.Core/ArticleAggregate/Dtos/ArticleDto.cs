namespace Server.Core.ArticleAggregate.Dtos;

public record ArticleDto(
  string Slug,
  string Title,
  string Description,
  string Body,
  List<string> TagList,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  bool Favorited,
  int FavoritesCount,
  AuthorDto Author
);
