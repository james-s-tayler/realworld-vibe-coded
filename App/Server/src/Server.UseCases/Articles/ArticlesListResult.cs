namespace Server.UseCases.Articles;

public record ArticlesListResult(
  List<ArticleResult> Articles,
  int ArticlesCount);
