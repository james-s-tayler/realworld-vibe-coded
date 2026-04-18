using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Infrastructure.Data;
using Server.UseCases.Interfaces;
using Server.Web.Shared.Pagination;

namespace Server.Web.Articles.List;

public class ListArticlesMapper : PaginatedResponseMapper<Article, ArticleDto>
{
  protected override Task<ArticleDto> MapItemAsync(Article entity, CancellationToken ct) =>
    ArticleMapper.MapArticleToDtoAsync(entity, Resolve<IUserContext>(), Resolve<AppDbContext>(), ct);
}
