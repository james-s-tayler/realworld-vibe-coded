using Server.Core.Interfaces;

namespace Server.Infrastructure.Data.Queries;

public class ListTagsQueryService(AppDbContext context) : IListTagsQueryService
{
  public async Task<IEnumerable<string>> ListAsync()
  {
    return await context.Tags
      .AsNoTracking()
      .Select(t => t.Name)
      .ToListAsync();
  }
}
