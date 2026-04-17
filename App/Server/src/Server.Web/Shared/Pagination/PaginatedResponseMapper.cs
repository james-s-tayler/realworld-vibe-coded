using Server.SharedKernel.Pagination;

namespace Server.Web.Shared.Pagination;

public abstract class PaginatedResponseMapper<TEntity, TItem>
  : ResponseMapper<PaginatedResponse<TItem>, PagedResult<TEntity>>
{
  public sealed override async Task<PaginatedResponse<TItem>> FromEntityAsync(
    PagedResult<TEntity> paged,
    CancellationToken ct)
  {
    var items = new List<TItem>(paged.Items.Count);
    foreach (var entity in paged.Items)
    {
      items.Add(await MapItemAsync(entity, ct));
    }

    return new PaginatedResponse<TItem>(items, paged.TotalCount);
  }

  protected abstract Task<TItem> MapItemAsync(TEntity entity, CancellationToken ct);
}
