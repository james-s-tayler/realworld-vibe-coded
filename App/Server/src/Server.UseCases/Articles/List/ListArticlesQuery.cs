using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.List;

public record ListArticlesQuery(
  string? Tag = null,
  string? Author = null,
  string? Favorited = null,
  int Limit = 20,
  int Offset = 0,
  Guid? CurrentUserId = null
) : IQuery<ListArticlesResult>;
