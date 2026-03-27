using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.List;

public record ListArticlesQuery(
  Guid CurrentUserId,
  string? Author = null,
  string? Tag = null,
  string? Favorited = null,
  int Limit = 20,
  int Offset = 0) : IQuery<ArticlesListResult>;
