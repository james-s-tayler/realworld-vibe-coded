using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles;

/// <summary>
/// Result wrapper for list of articles (entities, not DTOs)
/// Used by MediatR handlers - mapping to DTOs happens in endpoints
/// </summary>
public record ArticlesResult(
  List<Article> Articles,
  int ArticlesCount
);
